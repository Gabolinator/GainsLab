using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Equipment;

//used to gather the data from a form 
public class EquipmentEditFormDTO : EquipmentFormDTO
{
    
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 

    public override FormType FormType => FormType.Edit;
 
    public string? UpdatedBy => FilledBy;

    public UpdateRequest UpdateRequest =>
        ApplyRequest  == Request.ApplyRequest ? UpdateRequest.Update : UpdateRequest.DontUpdate;
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
            Descriptor = dto.Descriptor is null
                ? new DescriptorEditDTO()
                : dto.Descriptor.FromGetDTO()
        };
    }

    public static EquipmentUpdateDTO ToUpdateDTO(this EquipmentEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var descriptor = dto.Descriptor is DescriptorEditDTO descriptorEditDto ?  descriptorEditDto : new DescriptorEditDTO();

        
        return new EquipmentUpdateDTO
        {
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            Descriptor = descriptor.ToUpdateDTO()
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