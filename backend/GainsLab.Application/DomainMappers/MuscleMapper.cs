using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DomainMappers;

public static class MuscleMapper
{
    public static MuscleRecord ToRecord(this MuscleEntity domain, IClock clock)
    {
        
      
        return new  MuscleRecord
        {
            GUID = domain.Id,
            Name = domain.Content.Name,
            BodySection = domain.Content.BodySection,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc, // do i set to todays date ? 
            CreatedBy = domain.CreationInfo.CreatedBy,
            
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? clock.UtcNow, // do i set to todays date ? 
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
             
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy,
            Descriptor = (DescriptorRecord)domain.Descriptor?.ToRecord(clock),
        };
    }

    /// <summary>
    /// Projects a descriptor Record into the corresponding domain entity.
    /// </summary>
    /// <param name="Record">Record retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the Record is absent.</returns>
    public static MuscleEntity? ToDomain(this MuscleRecord? Record)
    {
        if (Record == null)
        {
            return null;
        }

        
        var descriptor =  (BaseDescriptorEntity)Record.Descriptor?.ToDomain();
        
        var content = new MuscleContent
        {
            Name = Record.Name,
            BodySection = Record.BodySection
        };

        var audited = new AuditedInfo(
            Record.CreatedAtUtc,
            Record.CreatedBy,
            Record.UpdatedAtUtc,
            Record.UpdatedBy,
            Record.Version,
            Record.IsDeleted,
            Record.DeletedAt,
            Record.DeletedBy);

        return new MuscleEntity(
            content,
            new MuscleId(Record.GUID),
            audited,
            descriptor ?? new BaseDescriptorEntity(),
            Record.AntagonistGUIDs.Select(it => new MuscleId(it)),
            Record.Id);

        

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
}
