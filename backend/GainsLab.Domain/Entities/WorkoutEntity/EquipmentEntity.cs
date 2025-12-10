using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity;



/// <summary>
/// Immutable data required to describe a single equipment item.
/// </summary>
public record EquipmentContent( string Name) : IEntityContent<EquipmentContent>
{
    
    public EquipmentContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Equipment name is required.", nameof(Name));
        return this;
    }
}

/// <summary>
/// Aggregate root for equipment including descriptor metadata.
/// </summary>
public class EquipmentEntity : EntityBase<EquipmentId, EquipmentContent, AuditedInfo>, IDescribed<BaseDescriptorEntity>
{
    
    public  EquipmentEntity(
        EquipmentContent content, 
        EquipmentId id, 
        AuditedInfo creation, 
        BaseDescriptorEntity descriptor, 
        int dbId = -1) : base(id, content, creation,dbId )
    {
        
         Descriptor = descriptor;
    }
    

    
    public override EntityType Type => EntityType.Equipment;
    public BaseDescriptorEntity Descriptor { get; }
    public string Name => Content.Name;
    
    public override string ToString()
    {
        return $"Equipment - " +
               $"Name: {Name} | " +
               $"ID : {Id}";
    }
    
}
