using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.UpdateDto;

/// <summary>
/// DTO used for PATCH requests targeting muscle resources.
/// </summary>
public record MuscleUpdateDTO
{
    [StringLength(256, MinimumLength = 2)]
    public string? Name { get; init; }

    [StringLength(256)]
    public string? LatinName { get; init; }

    [EnumDataType(typeof(eBodySection))]
    public eBodySection? BodySection { get; init; }

    public DescriptorUpdateDTO? Descriptor { get; init; }

    public IReadOnlyList<Guid>? AntagonistIds { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; init; }

    public string? UpdatedBy { get; init; }
}
