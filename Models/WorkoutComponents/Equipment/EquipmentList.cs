using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core;

namespace GainsLab.Models.Core.WorkoutComponents;

public class EquipmentList : ComponentLists<Equipment> , IWorkoutComponent
{
    
    public EquipmentList()
    {
     
    }
    
    public EquipmentList(Equipment equipment)
    {
       AddComponent(equipment);
    } 

    
    public EquipmentList(IEnumerable<ComponentReference<Equipment>> components)
    {
        AddComponents(components.ToList());
    }
    
    public EquipmentList(IEnumerable<Equipment> components)
    {
        AddComponents(components.Select(it => it.Identifier).ToList());
    }
    
    public EquipmentList(IEnumerable<IIdentifier> components)
    {
        AddComponents(components.ToList());
    }
    public override eWorkoutComponents ComponentsType { get; set; } = eWorkoutComponents.Equipment;
    
    public EquipmentList GetDistinct() => new EquipmentList(this.Distinct());
    
    public eWorkoutComponents ComponentType => eWorkoutComponents.EquipmentList;
    public IComponentDescriptor Descriptor { get; set; } = new EmptyDescriptor();
    public IIdentifier Identifier { get; set; } = new EmptyIdentifier();
    
    public IWorkoutComponent Copy()
    {
        return new EquipmentList(this.CopyList());
    }
    
}