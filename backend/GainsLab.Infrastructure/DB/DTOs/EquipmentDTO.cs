using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Infrastructure.DB.DTOs;

[Table("Equipments")]
public class EquipmentDTO : IDto 
{
    [Key]
    public int Id { get; set; } //primary - auto increment
    
    public string Name { get; set; }

    public Guid GUID { get; set; }
    public int DescriptorID { get; set; } //from descriptor dto
    
    public DescriptorDTO Descriptor { get; set; } = null!;
    
    public DateTimeOffset CreatedAtUtc  { get; set; }
    public string CreatedBy  { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; } = null;
    public string? UpdatedBy { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; } = null;
    public string? DeletedBy { get; set; } = null;
    
    public long Version { get; set; } = 0;

    public int Iid => Id;
    public Guid Iguid => GUID;
    
    public override bool Equals(object? obj)
    {
        if (obj is not EquipmentDTO other) return false;
        return Guid.Equals(GUID,other.GUID) 
               && (DescriptorID == other.DescriptorID);
    }

    public override int GetHashCode() => HashCode.Combine(GUID, DescriptorID);
    
}