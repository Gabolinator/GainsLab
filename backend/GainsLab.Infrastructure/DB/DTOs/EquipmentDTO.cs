using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Infrastructure.DB.DTOs;

public class EquipmentDTO : BaseDto 
{
    [Key]
    public int Id { get; set; } //primary - auto increment
    
    public string Name { get; set; }

    public Guid GUID { get; set; }
    public int DescriptorID { get; set; } //from descriptor dto
    
    public DescriptorDTO Descriptor { get; set; } = null!;
    

    public override int Iid => Id;
    public override Guid Iguid => GUID;
    
    public override bool Equals(object? obj)
    {
        if (obj is not EquipmentDTO other) return false;
        return Guid.Equals(GUID,other.GUID) 
               && (DescriptorID == other.DescriptorID);
    }

    public override int GetHashCode() => HashCode.Combine(GUID, DescriptorID);


    public override string ToString()
    {
        return $"Equipment DTO - " +
               $"Name: {Name}" +
               $"ID : {Id} | " +
               $"Guid :{GUID} ";
    }
}