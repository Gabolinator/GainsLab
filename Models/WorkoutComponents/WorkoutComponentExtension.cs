using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;

namespace GainsLab.Models.WorkoutComponents;

public static class WorkoutComponentExtension
{
    
    public static T CopyAs<T>(this IWorkoutComponent component) where T : IWorkoutComponent
    {
        return (T)component.Copy();
    }
    
    public static EquipmentList ToComponentList(this Equipment equipment, params Equipment[] others)
    {
        var list = new EquipmentList {Items = new(){ComponentReference<Equipment>.FromComponent(equipment)} };
        foreach (var other in others)
            list.AddComponent(other);
        return list;
    }

    public static MuscleList ToComponentList(this Muscle muscle, params Muscle[] others)
    {
        var list = new MuscleList { Items = new(){ComponentReference<Muscle>.FromComponent(muscle)} };
        foreach (var other in others)
            list.AddComponent(other);
        return list;
    }
    
}