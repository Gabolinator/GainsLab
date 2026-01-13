using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PutDto;

/// <summary>
/// DTO used for PUT operations on movement category resources.
/// </summary>
public record MovementCategoryPutDTO
{
    public Guid? Id { get; set; }

    public UpsertOutcome Outcome { get; set; } = UpsertOutcome.Failed;

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    public Guid? ParentCategoryId { get; set; }

    [Required]
    [MinLength(1)]
    public IReadOnlyList<eMovementCategories> BaseCategories { get; set; } =
        Array.Empty<eMovementCategories>();

    [Required]
    public DescriptorPutDTO Descriptor { get; set; } = default!;

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; set; } = DataAuthority.Bidirectional;

    public string? UpdatedBy { get; set; }
}
