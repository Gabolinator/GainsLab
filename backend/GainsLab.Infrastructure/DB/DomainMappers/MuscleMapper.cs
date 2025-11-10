using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;

namespace GainsLab.Infrastructure.DB.DomainMappers;

public static class MuscleMapper
{
    public static MuscleDTO ToDTO(this MuscleEntity domain)
    {
        return new  MuscleDTO
        {
            GUID = domain.Id,
            Name = domain.Content.Name,
            BodySection = domain.Content.BodySection,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc, // do i set to todays date ? 
            CreatedBy = domain.CreationInfo.CreatedBy,
            
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? CoreUtilities.Clock.UtcNow, // do i set to todays date ? 
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
             
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy,
            Descriptor = domain.Descriptor?.ToDTO()
        };
    }

    /// <summary>
    /// Projects a descriptor DTO into the corresponding domain entity.
    /// </summary>
    /// <param name="dto">DTO retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the DTO is absent.</returns>
    public static MuscleEntity? ToDomain(this MuscleDTO? dto)
    {
        if (dto == null)
        {
            return null;
        }

        
        var descriptor = dto.Descriptor?.ToDomain();
        
        var content = new MuscleContent
        {
            Name = dto.Name,
            BodySection = dto.BodySection
        };

        var audited = new AuditedInfo(
            dto.CreatedAtUtc,
            dto.CreatedBy,
            dto.UpdatedAtUtc,
            dto.UpdatedBy,
            dto.Version,
            dto.IsDeleted,
            dto.DeletedAt,
            dto.DeletedBy);

        return new MuscleEntity(content, new MuscleId(dto.GUID),audited ,descriptor ?? new BaseDescriptorEntity(),dto.AntagonistGUIDs.Select(it=> new MuscleId(it)));

        

    }
}
