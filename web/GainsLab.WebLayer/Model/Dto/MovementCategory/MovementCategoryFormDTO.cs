using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.MovementCategory;

public class MovementCategoryFormDTO
{
    public Guid  Id { get; set; }
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; } = "new name";

    public virtual DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();

    
    public MovementCategoryRefDTO? Parent { get; set; }

    public List<MovementCategoryRefDTO> BasesCategory { get; set; } = new();

    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 
    
    public virtual FormType FormType { get; }
    
    public Request ApplyRequest { get; set; }

    public string? FilledBy { get; set; }

    
}