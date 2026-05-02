using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity.EntityContent;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity;

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
