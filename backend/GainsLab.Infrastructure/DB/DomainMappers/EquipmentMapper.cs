using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;

namespace GainsLab.Infrastructure.DB.DomainMappers;

 public static class EquipmentMapper
 {
     public static EquipmentDTO ToDTO(EquipmentEntity domain)
     {
         //if id is negative - likely not yet inserted in db, dont keep id 
         //id id is positive - probably already inserted  - we keep it 
         return domain.DbId <= 0 ? ToInsertDTO(domain) : ToUpdateDTO(domain);
     }
     
     public static EquipmentDTO ToInsertDTO(EquipmentEntity domain)
     {

         return new EquipmentDTO
         {
             //dont use id 
             Name = domain.Name!,
             GUID = domain.Id,
             DescriptorID = domain.Descriptor.DbId,
             Descriptor = domain.Descriptor.ToDTO(),
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
     public static EquipmentDTO ToUpdateDTO(EquipmentEntity domain)
     {
         return new EquipmentDTO
         {
             Id = domain.DbId,  // Use existing Id
             GUID = domain.Id,
             DescriptorID = domain.Descriptor.DbId,
             Descriptor = domain.Descriptor.ToDTO(),
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

     public static EquipmentEntity ToDomain(EquipmentDTO dto)
     {
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
                 ), dto.Descriptor.ToDomain(),
             dto.Id);

     }
}