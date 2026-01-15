using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DomainMappers;

/// <summary>
/// Converts descriptor aggregates to and from their DTO representations.
/// </summary>
public static class DescriptorMapper
{

    public static DescriptorGetDTO? ToGetDTO(this DescriptorRecord? dto) =>
        dto.TryMapToGetDTO(out var mapped) ? mapped : null;

    public static bool TryMapToGetDTO(this DescriptorRecord? dto, out DescriptorGetDTO? mapped)
    {
        if (dto == null)
        {
            mapped = null;
            return false;
        }

        mapped = new DescriptorGetDTO(
            dto.GUID,
            dto.Content,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);
        return true;
    }
    
    public static DescriptorRecord? ToEntity(this DescriptorPostDTO? dto, IClock clock) =>
        dto.TryMapToEntity(clock, out var mapped) ? mapped : null;

    public static bool TryMapToEntity(this DescriptorPostDTO? dto, IClock clock, out DescriptorRecord? mapped)
    {
        if (dto == null)
        {
            mapped = null;
            return false;
        }

        mapped = new DescriptorRecord
        {
            GUID = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Content = dto.DescriptionContent,
            CreatedAtUtc = clock.UtcNow,
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy) ? dto.CreatedBy : "system",
            UpdatedAtUtc = clock.UtcNow,
            UpdatedSeq = 1,
            UpdatedBy = dto.CreatedBy,
            Authority = dto.Authority
        };

        return true;
    }
    
    public static DescriptorPutDTO? ToPutDTO(this DescriptorRecord? dto, IClock clock, UpsertOutcome outcome) =>
        dto.TryMapToPutDTO(clock, outcome, out var mapped) ? mapped : null;

    public static bool TryMapToPutDTO(this DescriptorRecord? dto, IClock clock, UpsertOutcome outcome, out DescriptorPutDTO? mapped)
    {
        if (dto == null)
        {
            mapped = null;
            return false;
        }

        mapped = new DescriptorPutDTO
        {
            Id = dto.GUID,
            DescriptionContent = dto.Content,
            Notes = null,
            Tags = null,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            Outcome = outcome,
        };

        return true;
    }
    
    
    public static DescriptorRecord? ToEntity(this DescriptorPutDTO? dto, IClock clock, Guid? id = null, DescriptorRecord? record= null) =>
        dto.TryMapToEntity(clock, out var mapped, id, record) ? mapped : null;

    public static bool TryMapToEntity(
        this DescriptorPutDTO? dto,
        IClock clock,
        out DescriptorRecord? mapped,
        Guid? id = null,
        DescriptorRecord? record = null)
    {
        if (dto == null)
        {
            mapped = null;
            return false;
        }

        mapped = new DescriptorRecord
        {
            
            GUID = id == null || id.Value.Equals(Guid.Empty) ? Guid.NewGuid() : id.Value,
            Content = dto.DescriptionContent,
            CreatedAtUtc = record !=null ? record.CreatedAtUtc: clock.UtcNow,
            CreatedBy = record !=null ? record.CreatedBy : "system",
            UpdatedAtUtc = clock.UtcNow,
            UpdatedSeq = record !=null ? record.UpdatedSeq:1,
            UpdatedBy = dto.UpdatedBy,
            Authority =  dto.Authority
        };

        return true;
    }


    /// <summary>
    /// Projects a descriptor domain entity into a DTO suitable for persistence.
    /// </summary>
    /// <param name="domain">Descriptor entity to convert.</param>
    /// <returns>A DTO containing descriptor fields.</returns>
    public static DescriptorRecord ToRecord(this BaseDescriptorEntity domain, IClock clock) =>
        domain.TryMapToRecord(clock, out var record)
            ? record!
            : throw new ArgumentNullException(nameof(domain));

    public static bool TryMapToRecord(this BaseDescriptorEntity? domain, IClock clock, out DescriptorRecord? record)
    {
        if (domain == null)
        {
            record = null;
            return false;
        }

        record = new DescriptorRecord
        {
            Id = domain.DbId > 0 ? domain.DbId : 0,
            GUID = domain.Id,
            Content = domain.Content.Description != null ? domain.Content.Description.Text ?? "none" : "none",
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
            CreatedBy = domain.CreationInfo.CreatedBy,
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc ?? clock.UtcNow,
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy
        };

        return true;
    }

    /// <summary>
    /// Projects a descriptor DTO into the corresponding domain entity.
    /// </summary>
    /// <param name="dto">DTO retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the DTO is absent.</returns>
    public static BaseDescriptorEntity? ToDomain(this DescriptorRecord? dto) =>
        dto.TryMapToDomain(out var entity) ? entity : null;

    public static bool TryMapToDomain(this DescriptorRecord? dto, out BaseDescriptorEntity? entity)
    {
        if (dto == null)
        {
            entity = null;
            return false;
        }

        var description = new Description(dto.Content);
        var content = new BaseDescriptorContent { Description = description };
        var creation = new AuditedInfo(dto.CreatedAtUtc, dto.CreatedBy, dto.UpdatedAtUtc, dto.UpdatedBy, dto.Version, dto.IsDeleted, dto.DeletedAt, dto.DeletedBy);

        entity = new BaseDescriptorEntity(new DescriptorId(dto.GUID), content, creation, dto.Id);
        return true;
    }
}
