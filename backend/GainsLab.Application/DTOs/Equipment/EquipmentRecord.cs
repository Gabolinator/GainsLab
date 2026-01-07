using System.ComponentModel.DataAnnotations;
using GainsLab.Application.DTOs.Description;
using GainsLab.Domain;

namespace GainsLab.Application.DTOs.Equipment;

public class EquipmentRecord : BaseRecord 
{
    [Key]
    public int Id { get; set; } //primary - auto increment
    
    public string Name { get; set; }

    public Guid GUID { get; set; }
    public int DescriptorID { get; set; } //from descriptor dto
    
    public DescriptorRecord? Descriptor { get; set; } = null!;


    public override string? GetContent() => Name;
    

    public override int Iid => Id;
    public override Guid Iguid => GUID;
    public override EntityType Type => EntityType.Equipment;

    public override bool Equals(object? obj)
    {
        if (obj is not EquipmentRecord other) return false;
        return (Guid.Equals(GUID,other.GUID) 
                || string.Equals(Name, other.Name, StringComparison.InvariantCultureIgnoreCase)) 
               && (DescriptorID == other.DescriptorID);
    }

    public override int GetHashCode() => HashCode.Combine(GUID, DescriptorID);


    public override string ToString()
    {
        return $"{nameof(EquipmentRecord)} - " +
               $"Name: {Name} |" +
               $"ID : {Id} | " +
               $"Guid :{GUID} ";
    }
}