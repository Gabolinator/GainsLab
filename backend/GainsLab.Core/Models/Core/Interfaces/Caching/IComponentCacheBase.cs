using GainsLab.Core.Models.Core;
using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.Caching.Interface;


/// <summary>
/// Base contract for component caches regardless of identifier type.
/// </summary>
public interface IComponentCacheBase
{
        
    void Clear();
        
    eWorkoutComponents ComponentType { get; }
    
}
