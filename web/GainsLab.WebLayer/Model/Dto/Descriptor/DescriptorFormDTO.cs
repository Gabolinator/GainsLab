using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.WebLayer.Model.Dto.Descriptor;

public class DescriptorFormDTO
{
    public Guid  Id { get; init; } = Guid.NewGuid();
    
    [StringLength(1024, MinimumLength = 3)]
    public string? DescriptionContent { get; set; } = "new description";

    [StringLength(2048)] public string? Notes { get; set; } = null;

    [MaxLength(20)] public ICollection<string>? Tags { get; set; } = null;
    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 
    
    public virtual FormType FormType { get; }
    
    public Request ApplyRequest { get; set; }

    public string? FilledBy { get; set; }
}