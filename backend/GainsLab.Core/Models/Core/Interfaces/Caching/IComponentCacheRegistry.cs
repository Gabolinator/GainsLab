using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Interfaces.Caching
{
    public interface IComponentCacheRegistry
    {
        public Task InitializeAsync();
        
       
        bool TryGetCache<TId, TEntity>(out  IComponentCache<TId, TEntity> componentCache);

        bool TryGetComponent<TId, TEntity>(TId id, out TEntity? component);
        bool TryGetComponents<TId, TEntity>(List<TId> ids, out List<TEntity>? components) ;
        
        void Store<TEntity>(TEntity component);
        void StoreAll<TEntity>(List<TEntity> components);

        public void StoreAll<TEntity>(eWorkoutComponents type, List<TEntity> components);
        
        void Remove<TId, TEntity>(TId id) ;
        void RemoveAll<TId, TEntity>(List<TId> ids);
       
        void ClearCache<TEntity>();
        void ClearAllCaches();
    }
}