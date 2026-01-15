using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Extensions;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DTOs.Equipment;


    public static class  EquipmentDTOExtension
    {
    
        public static bool AnythingChanged(this EquipmentRecord record, EquipmentPutDTO dto)
        {
            //can add other criterias as needed - ignore tags etc for now
            return !string.Equals(
                       record.GetContent(), 
                       dto.Name, 
                       StringComparison.InvariantCultureIgnoreCase) 
                   || record.Authority !=  dto.Authority;
        }
    
   
    
        public static bool TryUpdate(this EquipmentRecord record, EquipmentUpdateDTO dto ,IClock clock)
        {
            bool anyUpdate = false;
            if (!string.IsNullOrWhiteSpace(dto.Name) &&  !string.Equals(
                    record.GetContent(), 
                    dto.Name, 
                    StringComparison.InvariantCultureIgnoreCase))
            {
                record.Name = dto.Name;
                anyUpdate = true;
            }
        
           
            var descriptor =  record.Descriptor;
         
            if ( descriptor != null && dto.Descriptor != null && descriptor.AnythingChanged(dto.Descriptor) )
            {
                if (descriptor.TryUpdate(dto.Descriptor!, clock))
                {
                    record.Descriptor = descriptor;
                    anyUpdate = true;
                }
            }
            
            if (dto.Authority != null && record.Authority !=  dto.Authority)
            {
                record.Authority = dto.Authority.Value;
                anyUpdate = true;
            }
        
            if(!anyUpdate) return false;
        
            record.UpdatedAtUtc = clock.UtcNow;
            record.UpdatedBy = dto.UpdatedBy;
        
            return true;
        }
    }
