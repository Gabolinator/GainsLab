using System.Collections.Generic;
using System.Linq;

namespace GainsLab.Models.Core.WorkoutComponents;

public class MuscleList : ComponentLists<Muscle>
{
    public override eWorkoutComponents ComponentsType { get; set; } = eWorkoutComponents.Muscle;
    
    public MuscleList()
    {
      
    }

    public MuscleList(Muscle muscle)
    {
       
        AddComponent(muscle);
    }
    
    public MuscleList(ComponentReference<Muscle> muscle)
    {
        var list =new List<ComponentReference<Muscle>>(){muscle};
        AddComponents(list);
    }
    
    public MuscleList(MuscleList list)
    {
        AddComponents(list);
    }

    public MuscleList(IEnumerable<ComponentReference<Muscle>> components)
    {
        AddComponents(components.ToList());
    }

    public MuscleList GetDistinct()
    {
        var distinct = this.Distinct().ToList();
        return new MuscleList(distinct);
    }
    
    public override string ToString()
    {
        var resolved = this.Where(m => m.IsComponentResolved);
        return $"MuscleList: Count={Count} [{string.Join(", ", this.Select(m=> m.ToString()))}], Resolved={resolved.Count()} [{string.Join(", ", resolved.Select(m=> m.ToString()))}] ";
    }
    
}