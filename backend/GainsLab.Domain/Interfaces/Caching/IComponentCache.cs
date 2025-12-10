namespace GainsLab.Domain.Interfaces.Caching;

/// <summary>
/// Defines cache behavior for storing domain components keyed by identifier.
/// </summary>
public interface IComponentCache<TId, TEntity> : IComponentCacheBase
{
    /// <summary>
    /// Gets or sets the underlying dictionary used to back the cache.
    /// </summary>
    Dictionary<TId, TEntity> CachedComponents { get; set; }

    /// <summary>
    /// Attempts to retrieve an entity from the cache by identifier.
    /// </summary>
    /// <param name="id">Identifier of the entity to look up.</param>
    /// <param name="component">When successful, contains the cached entity.</param>
    /// <returns><c>true</c> if the entity exists in cache; otherwise <c>false</c>.</returns>
    bool TryGet(TId id, out TEntity? component);

    /// <summary>
    /// Stores the supplied entity in cache using its inferred identifier.
    /// </summary>
    void Store(TEntity entity);
    
    /// <summary>
    /// Inserts or replaces an entity using the provided identifier.
    /// </summary>
    /// <param name="id">Identifier to associate with the entity.</param>
    /// <param name="component">Entity instance to cache.</param>
    void StoreOrReplace(TId id, TEntity component);

    /// <summary>
    /// Removes a cached entity by identifier.
    /// </summary>
    /// <param name="id">Identifier associated with the cached entity.</param>
    void Remove(TId id);
    
}
