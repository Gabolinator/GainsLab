using GainsLab.Application.DTOs;
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

    public static DescriptorGetDTO? ToGetDTO(this DescriptorRecord? dto)
    {
        if (dto == null) return null;

        return new DescriptorGetDTO(
            dto.GUID,
            dto.Content,
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);

    }
    
    public static DescriptorRecord? ToEntity(this DescriptorPostDTO? dto, IClock clock)
    {
        if (dto == null) return null;

        return new DescriptorRecord
        {
            GUID = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Content = dto.DescriptionContent,
            CreatedAtUtc = clock.UtcNow,
            CreatedBy = !string.IsNullOrWhiteSpace(dto.CreatedBy)? dto.CreatedBy : "system",
            UpdatedAtUtc = clock.UtcNow,
            UpdatedSeq = 1,
            UpdatedBy = dto.CreatedBy,
            Authority =  dto.Authority
        };


    }
    
    public static DescriptorPutDTO? ToPutDTO(this DescriptorRecord? dto, IClock clock)
    {
        if (dto == null) return null;

        return new DescriptorPutDTO{
           Id= dto.GUID,
           DescriptionContent = dto.Content,
            Notes = null,
            Tags =  null,
           Authority = dto.Authority,
           UpdatedBy = dto.UpdatedBy};
        
    }
    
    
    public static DescriptorRecord? ToEntity(this DescriptorPutDTO? dto, IClock clock, Guid? id = null, DescriptorRecord? record= null)
    {
        if (dto == null) return null;

        return new DescriptorRecord
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


    }


    /// <summary>
    /// Projects a descriptor domain entity into a DTO suitable for persistence.
    /// </summary>
    /// <param name="domain">Descriptor entity to convert.</param>
    /// <returns>A DTO containing descriptor fields.</returns>
    public static DescriptorRecord ToRecord(this BaseDescriptorEntity domain, IClock clock)
    {
        return new DescriptorRecord
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
    }

    /// <summary>
    /// Projects a descriptor DTO into the corresponding domain entity.
    /// </summary>
    /// <param name="dto">DTO retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the DTO is absent.</returns>
    public static BaseDescriptorEntity? ToDomain(this DescriptorRecord? dto)
    {
        if (dto == null)
        {
            return null;
        }

        var description = new Description(dto.Content);
        var content = new BaseDescriptorContent { Description = description };
        var creation = new AuditedInfo(dto.CreatedAtUtc, dto.CreatedBy, dto.UpdatedAtUtc, dto.UpdatedBy, dto.Version, dto.IsDeleted, dto.DeletedAt, dto.DeletedBy);

        return new BaseDescriptorEntity(new DescriptorId(dto.GUID), content, creation, dto.Id);

        

    }
}
