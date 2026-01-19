using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.UpdateDto;

/// <summary>
/// DTO used for PATCH requests targeting movement category resources.
/// </summary>
public record MovementCategoryUpdateDTO
{
    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; init; }

    public MovementCategoryRefDTO? ParentCategory { get; init; }
    
    public IReadOnlyList<MovementCategoryRefDTO>? BaseCategories { get; init; }

    public DescriptorUpdateDTO? Descriptor { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; init; }

    public string? UpdatedBy { get; init; }
    
}



