using System.Collections.Generic;
using System.Threading.Tasks;

namespace GainsLab.Models.Core.Interfaces;

public interface IComponentList
{
    eWorkoutComponents ComponentsType { get; }
    IEnumerable<ComponentReference<IWorkoutComponent>> References { get; }
    
    IEnumerable<ComponentReference<IWorkoutComponent>> GetUnresolvedReferences();
    
    void AddComponent(ComponentReference<IWorkoutComponent> component);

    string ToString();
    
    


}