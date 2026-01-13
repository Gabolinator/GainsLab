using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PutDto;

/// <summary>
/// DTO used for PUT operations on muscle resources.
/// </summary>
public record MusclePutDTO
{
    public Guid? Id { get; set; }

    public UpsertOutcome Outcome { get; set; } = UpsertOutcome.Failed;

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    [StringLength(256)]
    public string? LatinName { get; set; }

    [Required]
    [EnumDataType(typeof(eBodySection))]
    public eBodySection BodySection { get; set; } = eBodySection.undefined;

    public IReadOnlyList<Guid>? AntagonistIds { get; set; }

    [Required]
    public DescriptorPutDTO Descriptor { get; set; } = default!;

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; set; } = DataAuthority.Bidirectional;

    public string? UpdatedBy { get; set; }
}
