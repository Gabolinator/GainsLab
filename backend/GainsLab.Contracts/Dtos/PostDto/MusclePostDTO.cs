using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PostDto;

/// <summary>
/// Payload used to create a new muscle along with descriptor metadata.
/// </summary>
public record MusclePostDTO
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(256)]
    public string? LatinName { get; init; }

    [Required]
    [EnumDataType(typeof(eBodySection))]
    public eBodySection BodySection { get; init; } = eBodySection.undefined;

    public DescriptorPostDTO? Descriptor { get; init; }

    public IReadOnlyList<Guid>? AntagonistIds { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; init; } = DataAuthority.Bidirectional;

    public string? CreatedBy { get; init; }
}
