using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.DataManagement.Caching.Interface;

namespace GainsLab.Core.Models.Core.Interfaces.Caching;

/// <summary>
/// Defines cache behavior for storing domain components keyed by identifier.
/// </summary>
public interface IComponentCache<TId, TEntity> : IComponentCacheBase
{
    Dictionary<TId, TEntity> CachedComponents { get; set; }
    bool TryGet(TId id, out TEntity? component);
    void Store(TEntity entity);
    
    void StoreOrReplace(TId id, TEntity component);
    void Remove(TId id);
    
}
