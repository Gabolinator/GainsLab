using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.Caching.Interface;


public interface IComponentCacheBase
{
        
    void Clear();
        
    eWorkoutComponents ComponentType { get; }
    
}
