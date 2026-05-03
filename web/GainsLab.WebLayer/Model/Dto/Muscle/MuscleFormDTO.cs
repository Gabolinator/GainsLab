using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.SummaryDto;
using GainsLab.Domain;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Muscle;

public class MuscleFormDTO
{
    public Guid  Id { get; set; } =  CoreUtilities.GuidGenerator.New();
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; }

    public virtual DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();
    
    [Required] public eBodySection BodySection { get; set; }
    public List<MuscleSummaryDTO>? Antagonists { get; set; }
    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 
    
    public virtual FormType FormType { get; }
    
    public Request ApplyRequest { get; set; }

    public string? FilledBy { get; set; }

}