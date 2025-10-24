
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.Caching;

public abstract class BaseComponentCache<Tid, TEntity> : IComponentCache<Tid, TEntity> 
{
    protected ILogger _logger;
    public abstract eWorkoutComponents ComponentType { get; }
    public Dictionary<Tid, TEntity> CachedComponents { get; set; } = new();
    public void Clear()
    {
        throw new NotImplementedException();
    }


    public bool TryGet(Tid id, out TEntity? component)
    {
        throw new NotImplementedException();
    }

    public void Store(TEntity entity)
    {
        throw new NotImplementedException();
    }

    public void StoreOrReplace(Tid id, TEntity component)
    {
        throw new NotImplementedException();
    }

    public void Remove(Tid id)
    {
        throw new NotImplementedException();
    }

    // public bool TryGet(IIdentifier id, out T? component)
    // {
    //     if (id.IsEmpty())
    //     {
    //         _logger.LogWarning(nameof(ComponentCacheRegistry), $"Could not find item in cache - id is empty ");
    //         component = default;
    //         return false;
    //     }
    //
    //     _logger.Log("BaseComponentCache", $"Trying to get component {id.Slug}  from {ComponentType} cache ");
    //     bool found = CachedComponents.TryGetValue(id, out component);
    //     if(found)   _logger.Log("BaseComponentCache", $"Found component {id.Slug}  in {ComponentType} cache ");
    //     else  _logger.LogWarning(nameof(ComponentCacheRegistry), $"Could not find item  {id.Slug} in cache");
    //     
    //     return found;
    // }
    //
    // public void Store(IIdentifier id, T component)
    // {
    //     // Only add if not already present
    //     if (id.IsEmpty()) return;
    //     if (CachedComponents.TryAdd(id, component))
    //     {
    //         _logger.Log("BaseComponentCache", $"Adding component {component.Name}  to {ComponentType} cache ");
    //
    //     }
    //     
    //     else  _logger.LogWarning("BaseComponentCache", $"Adding component {component.Name}  to {ComponentType} cache Failed - Element already exist in cache");
    //
    // }
    //
    // public void StoreOrReplace(IIdentifier id, T component)
    // {
    //     if (id.IsEmpty()) return;
    //     // Add or replace regardless
    //     _logger.Log("BaseComponentCache", $"Updated component {component.Name}  in {ComponentType} cache ");
    //
    //     CachedComponents[id] = component;
    // }
    //
    // public void Remove(IIdentifier id)
    // {
    //     //doesnt exist in cache, or id is empty
    //     if (!TryGet(id, out var value))
    //     {
    //         _logger.LogWarning("BaseComponentCache", $"Removing component {id.Slug}  from {ComponentType} cache Failed");
    //         return;
    //     }
    //     
    //     CachedComponents.Remove(id);
    //     _logger.Log("BaseComponentCache", $"Removing component {id.Slug}  from {ComponentType} cache");
    //
    //     
    // }
    //
    // public void Clear()
    // {
    //     _logger.Log("BaseComponentCache", $"Cleared all {ComponentType} cache");
    //     CachedComponents.Clear();
    // }
    //
    //
}