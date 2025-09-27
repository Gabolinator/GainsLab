using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.WorkoutComponents.Equipment;
using GainsLab.Models.WorkoutComponents.Muscle;

namespace GainsLab.Models.WorkoutComponents;

public static class WorkoutComponentExtension
{
    
    public static T CopyAs<T>(this IWorkoutComponent component) where T : IWorkoutComponent
    {
        return (T)component.Copy();
    }
    
    public static EquipmentList ToComponentList(this Equipment.Equipment equipment, params Equipment.Equipment[] others)
    {
        var list = new EquipmentList {Items = new(){ComponentReference<Equipment.Equipment>.FromComponent(equipment)} };
        foreach (var other in others)
            list.AddComponent(other);
        return list;
    }

    public static MuscleList ToComponentList(this Muscle.Muscle muscle, params Muscle.Muscle[] others)
    {
        var list = new MuscleList { Items = new(){ComponentReference<Muscle.Muscle>.FromComponent(muscle)} };
        foreach (var other in others)
            list.AddComponent(other);
        return list;
    }
    
}