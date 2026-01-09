using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Descriptor;

namespace GainsLab.WebLayer.Model.Equipment;

//used to gather the data from a form 
public class EquipmentEditFormDTO
{
    public Guid  Id { get; set; }
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; } = "new name";

    public DescriptorEditDTO Descriptor { get; set; } = new DescriptorEditDTO();

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 

    public string? UpdatedBy { get; set; }
    
    public UpdateRequest UpdateRequest { get; set; } = UpdateRequest.DontUpdate;
}


public static class EquipmentEditFormDTOExtensions
{
    public static EquipmentEditFormDTO FromGetDTO(this EquipmentGetDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new EquipmentEditFormDTO
        {
            Id = dto.Id,
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = null, // not present on Get DTO (unless you want to set from context elsewhere)
            Descriptor = dto.Descriptor is null
                ? new DescriptorEditDTO()
                : dto.Descriptor.FromGetDTO()
        };
    }

    public static EquipmentUpdateDTO ToUpdateDTO(this EquipmentEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new EquipmentUpdateDTO
        {
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            Descriptor = dto.Descriptor is null
                ? null
                : dto.Descriptor.ToUpdateDTO()
        };
    }
    
    public static EquipmentUpdateRequest ToUpdateRequest(this EquipmentEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new EquipmentUpdateRequest(
            dto.ToUpdateDTO(),
            dto.Id, 
            dto.UpdateRequest ,
            dto.UpdatedBy ?? "unknown" );

    }
}