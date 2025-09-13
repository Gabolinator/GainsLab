using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace GainsLab.Models.DataManagement.DB.Model.DTOs;

[Table("Equipments")]
public class EquipmentDTO : IDto
{
    [Key]
    public int Id { get; set; } //primary - auto increment
    public string UID { get; set; } //human readable id 
    public int DescriptorID { get; set; } //from descriptor dto
    
    public ComponentDescriptorDTO Descriptor { get; set; } = null!;

    public int Iid => Id;
    public string Iuid => UID;
    
    public override bool Equals(object? obj)
    {
        if (obj is not EquipmentDTO other) return false;
        return string.Equals(UID,other.UID, StringComparison.InvariantCultureIgnoreCase) 
               && (DescriptorID == other.DescriptorID);
    }

    public override int GetHashCode() => HashCode.Combine(UID, DescriptorID);
    
}