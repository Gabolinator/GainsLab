using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Models.DataManagement.DB.Model.DomainMappers;

/// <summary>
/// Converts descriptor aggregates to and from their DTO representations.
/// </summary>
public static class DescriptorMapper
{
    /// <summary>
    /// Projects a descriptor domain entity into a DTO suitable for persistence.
    /// </summary>
    /// <param name="domain">Descriptor entity to convert.</param>
    /// <returns>A DTO containing descriptor fields.</returns>
    public static DescriptorDTO ToDTO(this BaseDescriptorEntity domain)
    {
        return new DescriptorDTO
        {
          GUID = domain.Id,
          Content = domain.Content.Description != null ? domain.Content.Description.Text?? "none" : "none",
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
    /// Projects a descriptor DTO into the corresponding domain entity.
    /// </summary>
    /// <param name="dto">DTO retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the DTO is absent.</returns>
    public static BaseDescriptorEntity? ToDomain(this DescriptorDTO? dto)
    {
        if (dto == null)
        {
            return null;
        }

        var description = new Description(dto.Content);
        
        return new BaseDescriptorEntity(new DescriptorId(dto.GUID), new BaseDescriptorContent{Description = description}, new AuditedInfo(DateTimeOffset.UtcNow, "unknown" ));

        

    }
}
