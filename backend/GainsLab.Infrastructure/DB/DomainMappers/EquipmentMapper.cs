using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;

namespace GainsLab.Infrastructure.DB.DomainMappers;

 /// <summary>
 /// Converts equipment domain entities to and from DTOs used by the data layer.
 /// </summary>
 public static class EquipmentMapper
 {
     /// <summary>
     /// Maps an equipment entity into a DTO for persistence.
     /// </summary>
     public static EquipmentDTO ToDTO(EquipmentEntity domain)
     {
         //if id is negative - likely not yet inserted in db, dont keep id 
         //id id is positive - probably already inserted  - we keep it 
         return domain.DbId <= 0 ? ToInsertDTO(domain) : ToUpdateDTO(domain);
     }
     
     /// <summary>
     /// Builds a DTO configured for insertion of a new equipment record.
     /// </summary>
     public static EquipmentDTO ToInsertDTO(EquipmentEntity domain)
     {

         return new EquipmentDTO
         {
             //dont use id 
             Name = domain.Name!,
             GUID = domain.Id,
             DescriptorID = domain.Descriptor.DbId,
             Descriptor = (DescriptorDTO)domain.Descriptor.ToDTO(),
             CreatedAtUtc = domain.CreationInfo.CreatedAtUtc, // do i set to todays date ? 
             CreatedBy = domain.CreationInfo.CreatedBy,
            
             UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? CoreUtilities.Clock.UtcNow, // do i set to todays date ? 
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
     public static EquipmentDTO ToUpdateDTO(EquipmentEntity domain)
     {
         return new EquipmentDTO
         {
             Id = domain.DbId,  // Use existing Id
             GUID = domain.Id,
             DescriptorID = domain.Descriptor.DbId,
             Descriptor =  (DescriptorDTO)domain.Descriptor.ToDTO(),
             CreatedAtUtc = domain.CreationInfo.CreatedAtUtc,
             CreatedBy = domain.CreationInfo.CreatedBy,
             IsDeleted = domain.CreationInfo.IsDeleted,
             DeletedAt = domain.CreationInfo.DeletedAt,
             DeletedBy = domain.CreationInfo.DeletedBy,
             UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? CoreUtilities.Clock.UtcNow, // do i set to todays date ? 
             UpdatedBy = domain.CreationInfo.UpdatedBy,
             Version = domain.CreationInfo.Version,
             
         };
     }

     /// <summary>
     /// Converts an equipment DTO into its corresponding domain entity.
     /// </summary>
     public static EquipmentEntity? ToDomain(EquipmentDTO? dto)
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
