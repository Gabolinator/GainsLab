using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core;

namespace GainsLab.Models.WorkoutComponents.Muscle;

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
        var distinct = this.Items.Distinct().ToList();
        return new MuscleList(distinct);
    }
    
    public override string ToString()
    {
        var resolved = this.Items.Where(m => m.IsComponentResolved);
        return $"MuscleList: Count={Items.Count} [{string.Join(", ", this.Items.Select(m=> m.ToString()))}], Resolved={resolved.Count()} [{string.Join(", ", resolved.Select(m=> m.ToString()))}] ";
    }
    
}