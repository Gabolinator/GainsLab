using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.WorkoutComponents.Equipment;

public class EquipmentList : ComponentLists<Equipment> , IWorkoutComponent
{
    
    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public EquipmentList()
    {
        Identifier = new EmptyIdentifier();
        Descriptor = new ComponentDescriptor();
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
    
    public List<ComponentReference<Equipment>> Equipments
    {
        get => Items;
        set => Items = value;
    }
    
    public EquipmentList GetDistinct() => new EquipmentList(Equipments.Distinct());
    
    public eWorkoutComponents ComponentType => eWorkoutComponents.EquipmentList;
    public ComponentDescriptor Descriptor { get; set; } = new EmptyDescriptor();
    public Identifier Identifier { get; set; } = new EmptyIdentifier();
    
    public IWorkoutComponent Copy()
    {
        return new EquipmentList(Items);
    }
    
}