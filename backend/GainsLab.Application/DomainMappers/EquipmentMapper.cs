using GainsLab.Application.DTOs;
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
