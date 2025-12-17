using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.UpdateDto;

public record DescriptorUpdateDTO
{
    [StringLength(1024, MinimumLength = 3)]
    public string? DescriptionContent { get; init; }

    [StringLength(2048)]
    public string? Notes { get; init; }

    [MaxLength(20)]
    public IReadOnlyList<string>? Tags { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; init; }
}
