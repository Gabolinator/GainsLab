using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Muscle;

public class MuscleEditFormDTO : MuscleFormDTO
{

    
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();
    public override FormType FormType => FormType.Edit;
 
    public string? UpdatedBy => FilledBy;

    public UpdateRequest UpdateRequest =>
        ApplyRequest  == Request.ApplyRequest ? UpdateRequest.Update : UpdateRequest.DontUpdate;

   

    public override string ToString()
    {
        return $"{Id.ToString()} -  {Name} - Antagonists : {string.Join(',', Antagonists)}";
    }
}

public static class MuscleEditFormDTOExtensions
{


    public static MuscleEditFormDTO FromGetDTO(this MuscleGetDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MuscleEditFormDTO
        {

            Id = dto.Id,
            Name = dto.Name,
            BodySection = dto.BodySection,
            Antagonists = dto.Antagonists?.ToList(),
            Authority = dto.Authority,
            Descriptor = dto.Descriptor is null
                ? new DescriptorEditDTO()
                : dto.Descriptor.FromGetDTO()
        };
    }

    public static MuscleUpdateDTO ToUpdateDTO(this MuscleEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var descriptor = dto.Descriptor is DescriptorEditDTO descriptorEditDto ?  descriptorEditDto : new DescriptorEditDTO();
        return new MuscleUpdateDTO
        {
            BodySection = dto.BodySection,
            AntagonistIds = dto.Antagonists == null || !dto.Antagonists.Any()
                ? null
                : dto.Antagonists.Select(m => m.Id).ToList(),
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            Descriptor = descriptor.ToUpdateDTO() 
        };
    }

    public static MuscleUpdateRequest ToUpdateRequest(this MuscleEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MuscleUpdateRequest(
            dto.ToUpdateDTO(),
            dto.Id,
            dto.UpdateRequest,
            dto.UpdatedBy ?? "unknown");

    }
}