using GainsLab.Application.DTOs.Description;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DTOs.Extensions;

public static class DescriptionUpdateExtension
{
    
    public static bool AnythingChanged(this DescriptorRecord record, DescriptorPutDTO dto)
    {
        //can add other criterias as needed - ignore tags etc for now
        return !string.Equals(
            record.GetContent(), 
            dto.DescriptionContent, 
            StringComparison.InvariantCultureIgnoreCase) 
               || record.Authority !=  dto.Authority;
    }
    
    public static bool AnythingChanged(this DescriptorRecord record, DescriptorUpdateDTO dto)
    {
        //can add other criterias as needed - ignore tags etc for now
        return !string.Equals(
                   record.GetContent(), 
                   dto.DescriptionContent, 
                   StringComparison.InvariantCultureIgnoreCase) 
               || record.Authority !=  dto.Authority;
    }
    
   
    
    public static bool TryUpdate(this DescriptorRecord record, DescriptorUpdateDTO dto, IClock clock)
    {
        bool anyUpdate = false;
        if (!string.IsNullOrWhiteSpace(dto.DescriptionContent) &&  !string.Equals(
                record.GetContent(), 
                dto.DescriptionContent, 
                StringComparison.InvariantCultureIgnoreCase))
        {
            record.Content = dto.DescriptionContent;
            anyUpdate = true;
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