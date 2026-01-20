using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;

namespace GainsLab.WebLayer.Model.Dto.Descriptor;

public class DescriptorEditDTO : DescriptorFormDTO
{
    public override FormType FormType => FormType.Edit;
 
    public string? UpdatedBy => FilledBy;

    public UpdateRequest UpdateRequest =>
        ApplyRequest  == Request.ApplyRequest ? UpdateRequest.Update : UpdateRequest.DontUpdate;
}


public static class DescriptorEditDTOExtensions
{
    public static DescriptorEditDTO FromGetDTO(this DescriptorGetDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new DescriptorEditDTO
        {
            Id =  dto.Id,
            DescriptionContent = dto.content,
            Notes = null,
            Tags = null,
            Authority = dto.Authority,
        };
    }

    public static DescriptorUpdateDTO ToUpdateDTO(this DescriptorEditDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new DescriptorUpdateDTO
        {
            DescriptionContent = dto.DescriptionContent,
            Notes = dto.Notes,
            Tags = dto.Tags?.ToList(),
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy
        };
    }
    
    public static DescriptorUpdateRequest ToUpdateRequest(this DescriptorEditDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        
        return new DescriptorUpdateRequest(dto.ToUpdateDTO(), dto.Id, dto.UpdateRequest ,dto.UpdatedBy ?? "unknown");
        
    }
}