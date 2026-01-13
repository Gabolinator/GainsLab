using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.UpdateDto;

/// <summary>
/// DTO used for PATCH requests targeting movement category resources.
/// </summary>
public record MovementCategoryUpdateDTO
{
    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; init; }

    public Guid? ParentCategoryId { get; init; }

    [MinLength(1)]
    public IReadOnlyList<eMovementCategories>? BaseCategories { get; init; }

    public DescriptorUpdateDTO? Descriptor { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; init; }

    public string? UpdatedBy { get; init; }
}
