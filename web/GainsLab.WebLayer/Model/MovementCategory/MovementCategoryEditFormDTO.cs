using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Descriptor;

namespace GainsLab.WebLayer.Model.MovementCategory;

public class MovementCategoryEditFormDTO
{
    public Guid  Id { get; set; }
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; } = "new name";

    public DescriptorEditDTO Descriptor { get; set; } = new DescriptorEditDTO();

    
    public MovementCategoryRefDTO? Parent { get; set; }

    public List<MovementCategoryRefDTO> BasesCategory { get; set; } = new();

    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 

    public string? UpdatedBy { get; set; }
    
    public UpdateRequest UpdateRequest { get; set; } = UpdateRequest.DontUpdate;

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
            UpdatedBy = null, // not present on Get DTO (unless you want to set from context elsewhere)
            Descriptor = dto.Descriptor is null
                ? new DescriptorEditDTO()
                : dto.Descriptor.FromGetDTO()
        };
    }

    public static MovementCategoryUpdateDTO ToUpdateDTO(this MovementCategoryEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MovementCategoryUpdateDTO
        {
            ParentCategory = dto.Parent,
            BaseCategories = dto.BasesCategory,
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            
            Descriptor = dto.Descriptor is null
                ? null
                : dto.Descriptor.ToUpdateDTO()
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