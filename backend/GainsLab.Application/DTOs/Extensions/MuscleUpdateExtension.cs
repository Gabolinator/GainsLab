using GainsLab.Application.DTOs.Muscle;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.DTOs.Extensions;

public static class MuscleUpdateExtension
{
    public  static bool TryUpdate(
        this MuscleRecord record,  
        MuscleUpdateDTO dto,
        IClock clock, 
        ILogger? logger)
    {
        logger?.Log(
            nameof(MuscleUpdateExtension),
            $"TryUpdate - Muscle:{record.GUID} Name:{record.Name} IncomingName:{dto.Name ?? "null"} Authority:{dto.Authority?.ToString() ?? "null"}");

        bool anyUpdate = false;

        if (!Domain.Comparison.StringComparison.Same(record.Name, dto.Name))
        {
            var oldName = record.Name;
            record.Name = dto.Name!;
            anyUpdate = true;
            logger?.Log(nameof(MuscleUpdateExtension),$"Name changed from '{oldName}' to '{record.Name}'");
        }
        
        if (AsAuthorityChanged(record.Authority, dto.Authority))
        {
            var oldAuthority = record.Authority;
            record.Authority = dto.Authority!.Value;
            anyUpdate = true;
            logger?.Log(nameof(MuscleUpdateExtension),$"Authority changed from {oldAuthority} to {record.Authority}");

        }

        if (BodySectionChanged(record.BodySection, dto.BodySection))
        {
            var oldBodySection = record.BodySection;
            record.BodySection = dto.BodySection!.Value;
            anyUpdate = true;
        }

     

        if(!anyUpdate)
        {
            logger?.Log(nameof(MuscleUpdateExtension),
                $"TryUpdate - No changes detected for Muscle:{record.GUID}");
            return false;
        }
        
        record.UpdatedAtUtc = clock.UtcNow;
        record.UpdatedBy = dto.UpdatedBy;
        logger?.Log(nameof(MuscleUpdateExtension),
            $"TryUpdate - Updated timestamps for Muscle:{record.GUID} at {record.UpdatedAtUtc:o} by {record.UpdatedBy ?? "unknown"}");
        
        return true;
        
     
        bool AsAuthorityChanged(DataAuthority oldAuthority, DataAuthority? newAuthority)
        {
            if(newAuthority == null) return false;
            return oldAuthority !=newAuthority.Value;
        }
        
        bool BodySectionChanged(eBodySection old, eBodySection? newBodySection)
        {
            if (newBodySection == null) newBodySection = eBodySection.undefined;
            return old != newBodySection;
        }
    }


}