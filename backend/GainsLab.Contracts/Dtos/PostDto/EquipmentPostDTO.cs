using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PostDto;

/// <summary>
/// Payload used to create a new equipment entity along with its descriptor.
/// </summary>
public record EquipmentPostDTO
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [Required]
    public DescriptorPostDTO Descriptor { get; init; } = default!;

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; init; } = DataAuthority.Bidirectional;

    public string? CreatedBy { get; init; }
}
