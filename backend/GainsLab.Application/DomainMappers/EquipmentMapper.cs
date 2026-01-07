using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DomainMappers;

/// <summary>
/// Converts equipment domain entities to and from DTOs used by the data layer.
/// </summary>
public static class EquipmentMapper
{
    /// <summary>
    /// Projects an EF Core equipment record into the Get DTO.
    /// </summary>
    public static EquipmentGetDTO? ToGetDTO(this EquipmentRecord? dto)
    {
        if (dto == null) return null;

        return new EquipmentGetDTO(
            dto.GUID,
            dto.Name,
            dto.Descriptor?.GUID,
            dto.Descriptor?.ToGetDTO(),
            dto.CreatedAtUtc,
            dto.UpdatedAtUtc,
            dto.UpdatedSeq,
            dto.IsDeleted,
            dto.Authority);
    }

    /// <summary>
    /// Maps a POST payload into an EF Core equipment record.
    /// </summary>
    public static EquipmentRecord? ToEntity(this EquipmentPostDTO? dto, IClock clock)
    {
        if (dto == null) return null;

        var descriptor = dto.Descriptor?.ToEntity(clock)
                         ?? throw new InvalidOperationException("Descriptor payload is required for equipment creation.");

        var createdBy = string.IsNullOrWhiteSpace(dto.CreatedBy) ? "system" : dto.CreatedBy!;

        return new EquipmentRecord
        {
            GUID = dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id,
            Name = dto.Name,
            Descriptor = descriptor,
            DescriptorID = descriptor.Iid,
            CreatedAtUtc = clock.UtcNow,
            CreatedBy = createdBy,
            UpdatedAtUtc = clock.UtcNow,
            UpdatedBy = createdBy,
            UpdatedSeq = 1,
            Authority = dto.Authority
        };
    }

    /// <summary>
    /// Projects an equipment record into a DTO returned by PUT handlers.
    /// </summary>
    public static EquipmentPutDTO? ToPutDTO(this EquipmentRecord? dto, IClock clock, UpsertOutcome outcome)
    {
        if (dto == null) return null;

        return new EquipmentPutDTO
        {
            Id = dto.GUID,
            Outcome = outcome,
            Name = dto.Name,
            Descriptor = dto.Descriptor?.ToPutDTO(clock, outcome) ?? new DescriptorPutDTO
            {
                Id = dto.Descriptor?.GUID,
                DescriptionContent = dto.Descriptor?.Content ?? string.Empty,
                Authority = dto.Descriptor?.Authority ?? dto.Authority,
                UpdatedBy = dto.Descriptor?.UpdatedBy
            },
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy
        };
    }

    /// <summary>
    /// Converts a PUT payload into an EF Core record, reusing existing values when supplied.
    /// </summary>
    public static EquipmentRecord? ToEntity(
        this EquipmentPutDTO? dto,
        IClock clock,
        Guid? id = null,
        EquipmentRecord? record = null)
    {
        if (dto == null) return null;

        var equipmentId = id == null || id == Guid.Empty
            ? (dto.Id == null || dto.Id == Guid.Empty ? Guid.NewGuid() : dto.Id.Value)
            : id.Value;

        var descriptor = dto.Descriptor?.ToEntity(clock, dto.Descriptor?.Id ?? record?.Descriptor?.GUID, record?.Descriptor)
                         ?? record?.Descriptor
                         ?? throw new InvalidOperationException("Descriptor payload is required for equipment upsert.");

        var createdAt = record?.CreatedAtUtc ?? clock.UtcNow;
        var createdBy = record?.CreatedBy ?? (dto.UpdatedBy ?? "system");

        return new EquipmentRecord
        {
            Id = record?.Id ?? 0,
            GUID = equipmentId,
            Name = dto.Name,
            Descriptor = descriptor,
            DescriptorID = descriptor.Iid,
            CreatedAtUtc = createdAt,
            CreatedBy = createdBy,
            UpdatedAtUtc = clock.UtcNow,
            UpdatedBy = dto.UpdatedBy,
            UpdatedSeq = record?.UpdatedSeq ?? 1,
            Authority = dto.Authority,
            Version = record?.Version ?? 0,
            IsDeleted = record?.IsDeleted ?? false,
            DeletedAt = record?.DeletedAt,
            DeletedBy = record?.DeletedBy
        };
    }

     /// <summary>
     /// Maps an equipment entity into a DTO for persistence.
     /// </summary>
     public static EquipmentRecord ToRecord(EquipmentEntity domain, IClock clock)
     {
         //if id is negative - likely not yet inserted in db, dont keep id 
         //id id is positive - probably already inserted  - we keep it 
         return domain.DbId <= 0 ? ToInsertRecord(domain, clock) : ToUpdateRecord(domain, clock);
     }
     
     /// <summary>
     /// Builds a DTO configured for insertion of a new equipment record.
     /// </summary>
     public static EquipmentRecord ToInsertRecord(EquipmentEntity domain, IClock clock)
     {

         return new EquipmentRecord
         {
             //dont use id 
             Name = domain.Name!,
             GUID = domain.Id,
             DescriptorID = domain.Descriptor.DbId,
             Descriptor = (DescriptorRecord)domain.Descriptor.ToRecord(clock),
             CreatedAtUtc = domain.CreationInfo.CreatedAtUtc, // do i set to todays date ? 
             CreatedBy = domain.CreationInfo.CreatedBy,
            
             UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? clock.UtcNow, // do i set to todays date ? 
             UpdatedBy = domain.CreationInfo.UpdatedBy,
             Version = domain.CreationInfo.Version,
             
             IsDeleted = domain.CreationInfo.IsDeleted,
             DeletedAt = domain.CreationInfo.DeletedAt,
             DeletedBy = domain.CreationInfo.DeletedBy,
            
             
         };
     }
     /// <summary>
     /// Builds a DTO configured to update an existing equipment record.
     /// </summary>
     public static EquipmentRecord ToUpdateRecord(EquipmentEntity domain, IClock clock)
     {
         return new EquipmentRecord()
         {
             Id = domain.DbId,  // Use existing Id
             GUID = domain.Id,
             DescriptorID = domain.Descriptor.DbId,
             Descriptor =  (DescriptorRecord)domain.Descriptor.ToRecord(clock),
             CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
             CreatedBy = domain.CreationInfo.CreatedBy,
             IsDeleted = domain.CreationInfo.IsDeleted,
             DeletedAt = domain.CreationInfo.DeletedAt,
             DeletedBy = domain.CreationInfo.DeletedBy,
             UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? clock.UtcNow, // do i set to todays date ? 
             UpdatedBy = domain.CreationInfo.UpdatedBy,
             Version = domain.CreationInfo.Version,
             
         };
     }

     /// <summary>
     /// Converts an equipment DTO into its corresponding domain entity.
     /// </summary>
     public static EquipmentEntity? ToDomain(EquipmentRecord? dto)
     {
         if (dto == null) return null;
         
         return new EquipmentEntity(
             new EquipmentContent(dto.Name),
             new EquipmentId(dto.GUID), 
             new AuditedInfo(
                 dto.CreatedAtUtc,
                 dto.CreatedBy,
                 dto.UpdatedAtUtc,
                 dto.UpdatedBy,
                 dto.Version,
                 dto.IsDeleted,
                 dto.DeletedAt,
                 dto.DeletedBy
                 ),  (BaseDescriptorEntity)dto.Descriptor.ToDomain(),
             dto.Id);

     }
}
