using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DomainMappers;

public static class MuscleMapper
{
    public static MuscleGetDTO? ToGetDTO(this MuscleRecord? record) =>
        record.TryMapToGetDTO(out var dto) ? dto : null;

    public static bool TryMapToGetDTO(this MuscleRecord? record, out MuscleGetDTO? dto)
    {
        if (record == null)
        {
            dto = null;
            return false;
        }

        record.Descriptor.TryMapToGetDTO(out var descriptorDto);
        var antagonistIds = record.AntagonistGUIDs?.ToArray();

        dto = new MuscleGetDTO(
            record.GUID,
            record.Name,
            null,
            record.BodySection,
            record.Descriptor?.GUID,
            descriptorDto,
            antagonistIds,
            null,
            record.CreatedAtUtc,
            record.UpdatedAtUtc,
            record.UpdatedSeq,
            record.IsDeleted,
            record.Authority);
        return true;
    }

    public static MuscleRefDTO ToRefDto(this MuscleGetDTO syncDto)
    {
        return new MuscleRefDTO(syncDto.Id, syncDto.Name, syncDto.LatinName);
    }
    
    public static MuscleRecord? ToEntity(this MusclePostDTO? dto, IClock clock) =>
        dto.TryMapToEntity(clock, out var record) ? record : null;

    public static bool TryMapToEntity(this MusclePostDTO? dto, IClock clock, out MuscleRecord? record)
    {
        if (dto == null)
        {
            record = null;
            return false;
        }

        dto.Descriptor.TryMapToEntity(clock, out var descriptorRecord);

        record = new MuscleRecord
        {
            GUID = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            BodySection = dto.BodySection,
            DescriptorID = descriptorRecord?.Id ?? 0,
            Descriptor = descriptorRecord,
            CreatedAtUtc = clock.UtcNow,
            CreatedBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "system" : dto.CreatedBy!,
            UpdatedAtUtc = clock.UtcNow,
            UpdatedBy = dto.CreatedBy,
            UpdatedSeq = 1,
            Authority = dto.Authority
        };

        return true;
    }

    public static MusclePutDTO? ToPutDTO(this MuscleRecord? record, IClock clock, UpsertOutcome outcome) =>
        record.TryMapToPutDTO(clock, outcome, out var dto) ? dto : null;

    public static bool TryMapToPutDTO(this MuscleRecord? record, IClock clock, UpsertOutcome outcome, out MusclePutDTO? dto)
    {
        if (record == null)
        {
            dto = null;
            return false;
        }

        record.Descriptor.TryMapToPutDTO(clock, outcome, out var descriptorPut);

        dto = new MusclePutDTO
        {
            Id = record.GUID,
            Outcome = outcome,
            Name = record.Name,
            LatinName = null,
            BodySection = record.BodySection,
            AntagonistIds = record.AntagonistGUIDs?.ToArray(),
            Descriptor = descriptorPut ?? new DescriptorPutDTO
            {
                Id = record.Descriptor?.GUID,
                DescriptionContent = record.Descriptor?.Content ?? string.Empty,
                Authority = record.Authority,
                Outcome = outcome
            },
            Authority = record.Authority,
            UpdatedBy = record.UpdatedBy
        };

        return true;
    }

    public static MuscleRecord? ToEntity(this MusclePutDTO? dto, IClock clock, Guid? id = null, MuscleRecord? record = null) =>
        dto.TryMapToEntity(clock, out var mapped, id, record) ? mapped : null;

    public static bool TryMapToEntity(
        this MusclePutDTO? dto,
        IClock clock,
        out MuscleRecord? record,
        Guid? id = null,
        MuscleRecord? existing = null)
    {
        if (dto == null)
        {
            record = null;
            return false;
        }

        dto.Descriptor.TryMapToEntity(clock, out var descriptorRecord);
        var descriptor = descriptorRecord ?? existing?.Descriptor;

        record = new MuscleRecord
        {
            Id = existing?.Id ?? 0,
            GUID = ResolveMuscleGuid(id, dto.Id, existing?.GUID),
            Name = dto.Name,
            BodySection = dto.BodySection,
            DescriptorID = descriptor?.Id ?? existing?.DescriptorID ?? 0,
            Descriptor = descriptor,
            CreatedAtUtc = existing?.CreatedAtUtc ?? clock.UtcNow,
            CreatedBy = existing?.CreatedBy ?? (dto.UpdatedBy ?? "system"),
            UpdatedAtUtc = clock.UtcNow,
            UpdatedBy = dto.UpdatedBy,
            UpdatedSeq = existing?.UpdatedSeq ?? 1,
            Authority = dto.Authority,
            RowVersion = existing?.RowVersion,
            Version = existing?.Version ?? 0,
            IsDeleted = existing?.IsDeleted ?? false,
            DeletedAt = existing?.DeletedAt,
            DeletedBy = existing?.DeletedBy,
            Antagonists = existing?.Antagonists != null
                ? new List<MuscleAntagonistRecord>(existing.Antagonists)
                : new List<MuscleAntagonistRecord>(),
            Agonists = existing?.Agonists != null
                ? new List<MuscleAntagonistRecord>(existing.Agonists)
                : new List<MuscleAntagonistRecord>()
        };

        return true;
    }

    public static MuscleRecord ToRecord(this MuscleEntity domain, IClock clock) =>
        domain.TryMapToRecord(clock, out var record)
            ? record!
            : throw new ArgumentNullException(nameof(domain));

    public static bool TryMapToRecord(this MuscleEntity? domain, IClock clock, out MuscleRecord? record)
    {
        if (domain == null)
        {
            record = null;
            return false;
        }

        domain.Descriptor.TryMapToRecord(clock, out var descriptorRecord);
        
        record = new MuscleRecord
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Name = domain.Content.Name,
            BodySection = domain.Content.BodySection,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? clock.UtcNow,
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy,
            DescriptorID = descriptorRecord?.Id ?? 0,
            Descriptor = descriptorRecord
        };

        return true;
    }

    /// <summary>
    /// Projects a descriptor Record into the corresponding domain entity.
    /// </summary>
    /// <param name="Record">Record retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the Record is absent.</returns>
    public static MuscleEntity? ToDomain(this MuscleRecord? Record) =>
        Record.TryMapToDomain(out var entity) ? entity : null;

    public static bool TryMapToDomain(this MuscleRecord? record, out MuscleEntity? entity)
    {
        if (record == null)
        {
            entity = null;
            return false;
        }

        record.Descriptor.TryMapToDomain(out var descriptor);
        descriptor ??= new BaseDescriptorEntity();
        
        var content = new MuscleContent
        {
            Name = record.Name,
            BodySection = record.BodySection
        };

        var audited = new AuditedInfo(
            record.CreatedAtUtc,
            record.CreatedBy,
            record.UpdatedAtUtc,
            record.UpdatedBy,
            record.Version,
            record.IsDeleted,
            record.DeletedAt,
            record.DeletedBy);

        entity = new MuscleEntity(
            content,
            new MuscleId(record.GUID),
            audited,
            descriptor,
            record.AntagonistGUIDs.Select(it => new MuscleId(it)),
            record.Id);

        return true;
    }

    public static IEnumerable<MuscleAntagonistRecord> CreateMuscleAntagonistRecords(
        IEnumerable<MuscleRecord> muscleRecords,
        IEnumerable<MuscleEntity> muscles)
    {
        if (muscleRecords == null) throw new ArgumentNullException(nameof(muscleRecords));
        if (muscles == null) throw new ArgumentNullException(nameof(muscles));

        var RecordById = muscleRecords
            .Where(Record => Record != null)
            .ToDictionary(Record => new MuscleId(Record.GUID));

        var links = new List<MuscleAntagonistRecord>();

        foreach (var muscle in muscles)
        {
            if (muscle == null || !muscle.AntagonistIds.Any()) continue;
            if (!RecordById.TryGetValue(muscle.Id, out var sourceRecord)) continue;

            foreach (var antagonistId in muscle.AntagonistIds)
            {
                if (!RecordById.TryGetValue(antagonistId, out var antagonistRecord)) continue;

                // Guard against duplicate links when the domain already contains them.
                var alreadyLinked = sourceRecord.Antagonists
                    .Any(link => link.Antagonist.GUID == antagonistRecord.GUID);
                if (alreadyLinked) continue;

                var link = new MuscleAntagonistRecord
                {
                    Muscle = sourceRecord,
                    Antagonist = antagonistRecord
                };

                sourceRecord.Antagonists.Add(link);
                antagonistRecord.Agonists.Add(link);
                links.Add(link);
            }
        }

        return links;
    }

    private static Guid ResolveMuscleGuid(Guid? overrideId, Guid? dtoId, Guid? existingId)
    {
        if (overrideId.HasValue && overrideId.Value != Guid.Empty) return overrideId.Value;
        if (dtoId.HasValue && dtoId.Value != Guid.Empty) return dtoId.Value;
        if (existingId.HasValue && existingId.Value != Guid.Empty) return existingId.Value;
        return Guid.NewGuid();
    }
}
