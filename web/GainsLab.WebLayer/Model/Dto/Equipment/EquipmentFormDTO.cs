using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Equipment;

public class EquipmentFormDTO
{
    public EquipmentId Id { get; init; } = EquipmentId.New();
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; } = "new name";

    public virtual DescriptorFormDTO? Descriptor { get; set; }
    
    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; } 
    
    public virtual FormType FormType { get; }
    
    public Request ApplyRequest { get; set; }

    public string? FilledBy { get; set; }
}