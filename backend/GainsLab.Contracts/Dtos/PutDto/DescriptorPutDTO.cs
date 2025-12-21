using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PutDto;

public record DescriptorPutDTO
{
    public Guid? Id { get; set; } =null;
    
    [Required]
    [StringLength(1024, MinimumLength = 3)]
    public string DescriptionContent { get; set; } = string.Empty;

    [StringLength(2048)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public IReadOnlyList<string>? Tags { get; set; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; set; } = DataAuthority.Bidirectional;

    public string? UpdatedBy { get; set; }
}
