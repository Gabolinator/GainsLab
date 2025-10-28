using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Interfaces.Caching
{
    /// <summary>
    /// Coordinates registration and retrieval of component caches for different entity types.
    /// </summary>
    public interface IComponentCacheRegistry
    {
        /// <summary>
        /// Initializes all registered caches (e.g., by pre-loading metadata).
        /// </summary>
        public Task InitializeAsync();
        
       
        /// <summary>
        /// Attempts to retrieve a typed cache for the specified entity identifier/value pair.
        /// </summary>
        bool TryGetCache<TId, TEntity>(out  IComponentCache<TId, TEntity> componentCache);

        /// <summary>
        /// Attempts to retrieve a single component from cache.
        /// </summary>
        bool TryGetComponent<TId, TEntity>(TId id, out TEntity? component);

        /// <summary>
        /// Attempts to retrieve multiple components from cache.
        /// </summary>
        bool TryGetComponents<TId, TEntity>(List<TId> ids, out List<TEntity>? components) ;

        /// <summary>
        /// Inserts or overwrites a single component in its corresponding cache.
        /// </summary>
        void Store<TEntity>(TEntity component);

        /// <summary>
        /// Inserts or overwrites a list of components in their inferred cache.
        /// </summary>
        void StoreAll<TEntity>(List<TEntity> components);

        /// <summary>
        /// Inserts or overwrites a list of components under the supplied entity type.
        /// </summary>
        public void StoreAll<TEntity>(EntityType type, List<TEntity> components);

        /// <summary>
        /// Removes a component from cache by identifier.
        /// </summary>
        void Remove<TId, TEntity>(TId id) ;

        /// <summary>
        /// Removes a set of components from cache by identifier.
        /// </summary>
        void RemoveAll<TId, TEntity>(List<TId> ids);

        /// <summary>
        /// Clears the cache for a single entity type.
        /// </summary>
        void ClearCache<TEntity>();

        /// <summary>
        /// Clears every registered cache.
        /// </summary>
        void ClearAllCaches();
    }
}
