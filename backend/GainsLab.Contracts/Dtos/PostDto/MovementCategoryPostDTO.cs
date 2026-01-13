using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PostDto;

/// <summary>
/// Payload used to create a movement category and its descriptor metadata.
/// </summary>
public record MovementCategoryPostDTO
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    public Guid? ParentCategoryId { get; init; }

    [Required]
    [MinLength(1)]
    public IReadOnlyList<eMovementCategories> BaseCategories { get; init; } =
        Array.Empty<eMovementCategories>();

    public DescriptorPostDTO? Descriptor { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; init; } = DataAuthority.Bidirectional;

    public string? CreatedBy { get; init; }
}
