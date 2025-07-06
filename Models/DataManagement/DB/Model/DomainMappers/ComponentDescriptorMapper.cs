using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Models.DataManagement.DB.Model.DomainMappers;

public static class ComponentDescriptorMapper
{
    public static ComponentDescriptorDTO ToDTO(this ComponentDescriptor domain)
    {
        return new ComponentDescriptorDTO
        {
          Name = domain.Name
        };
    }

    public static ComponentDescriptor ToDomain(this ComponentDescriptorDTO dto)
    {
        return new ComponentDescriptor()
            {
                Name = dto.Name
            }
        ;
    }
}