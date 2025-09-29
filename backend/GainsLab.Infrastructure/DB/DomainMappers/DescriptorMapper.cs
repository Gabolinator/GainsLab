using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Models.DataManagement.DB.Model.DomainMappers;

public static class DescriptorMapper
{
    public static DescriptorDTO ToDTO(this BaseDescriptorEntity domain)
    {
        return new DescriptorDTO
        {
          GUID = domain.Id
        };
    }

    public static BaseDescriptorEntity ToDomain(this DescriptorDTO dto)
    {
        return new BaseDescriptorEntity(new DescriptorId(dto.GUID), new BaseDescriptorContent(), new AuditedInfo(DateTimeOffset.UtcNow, "unknown" ));

        

    }
}