namespace GainsLab.Domain.Interfaces.Caching;


/// <summary>
/// Base contract for component caches regardless of identifier type.
/// </summary>
public interface IComponentCacheBase
{
    /// <summary>
    /// Clears all cached entries maintained by the current cache instance.
    /// </summary>
    void Clear();

    /// <summary>
    /// Gets the workout component type associated with items stored in this cache.
    /// </summary>
    eWorkoutComponents ComponentType { get; }
}
