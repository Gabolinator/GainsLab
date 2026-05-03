using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SummaryDto;
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.MovementCategory;

public class MovementCategoryFormDTO
{
    public MovementCategoryId  Id { get; set; } =  MovementCategoryId.New();
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; } = "new name";

    public virtual DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();

    
    public MovementCategorySummaryDTO? Parent { get; set; }

    public List<MovementCategorySummaryDTO> BasesCategory { get; set; } = new();

    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 
    
    public virtual FormType FormType { get; }
    
    public Request ApplyRequest { get; set; }

    public string? FilledBy { get; set; }

    
}