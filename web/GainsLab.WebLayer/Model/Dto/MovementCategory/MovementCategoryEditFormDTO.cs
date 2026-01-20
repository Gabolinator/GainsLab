using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.MovementCategory;

public class MovementCategoryEditFormDTO : MovementCategoryFormDTO
{
   
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();
    
    public override FormType FormType => FormType.Edit;
 
    public string? UpdatedBy => FilledBy;

    public UpdateRequest UpdateRequest =>
        ApplyRequest  == Request.ApplyRequest ? UpdateRequest.Update : UpdateRequest.DontUpdate;

    public override string ToString()
    {
        return $"{Id.ToString()} -  {Name} - Parent:  {(Parent != null ? Parent.Name : "none")} Bases : {string.Join(',', BasesCategory)}";
    }
}


public static class MovementCategoryEditFormDTOExtensions
{
   
    
    public static MovementCategoryEditFormDTO FromGetDTO(this MovementCategoryGetDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MovementCategoryEditFormDTO
        {
            Id = dto.Id,
            Name = dto.Name,
            Parent = dto.ParentCategory,
            BasesCategory = dto.BaseCategories != null? dto.BaseCategories.ToList() : new(),
            Authority = dto.Authority,
            Descriptor = dto.Descriptor is null
                ? new DescriptorEditDTO()
                : dto.Descriptor.FromGetDTO()
        };
    }

    public static MovementCategoryUpdateDTO ToUpdateDTO(this MovementCategoryEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var descriptor = dto.Descriptor is DescriptorEditDTO descriptorEditDto ?  descriptorEditDto : new DescriptorEditDTO();

        
        return new MovementCategoryUpdateDTO
        {
            ParentCategory = dto.Parent,
            BaseCategories = dto.BasesCategory,
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            Descriptor = descriptor.ToUpdateDTO()
        };
    }

    public static MovementCategoryUpdateRequest ToUpdateRequest(this MovementCategoryEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MovementCategoryUpdateRequest(
            dto.ToUpdateDTO(),
            dto.Id,
            dto.UpdateRequest,
            dto.UpdatedBy ?? "unknown");

    }
}