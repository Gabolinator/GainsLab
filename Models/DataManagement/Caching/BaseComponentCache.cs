using System.Collections.Generic;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement.Caching;

public abstract class BaseComponentCache<T> : IComponentCache<T> where T : IWorkoutComponent
{
    protected IWorkoutLogger _logger;

    public abstract eWorkoutComponents ComponentType { get; }
    public Dictionary<IIdentifier, T> CachedComponents { get; set; } = new();

    public bool TryGet(IIdentifier id, out T? component)
    {
        if (id.IsEmpty())
        {
            _logger.LogWarning(nameof(ComponentCacheRegistry), $"Could not find item in cache - id is empty ");
            component = default;
            return false;
        }

        _logger.Log("BaseComponentCache", $"Trying to get component {id.UID}  from {ComponentType} cache ");
        bool found = CachedComponents.TryGetValue(id, out component);
        if(found)   _logger.Log("BaseComponentCache", $"Found component {id.UID}  in {ComponentType} cache ");
        else  _logger.LogWarning(nameof(ComponentCacheRegistry), $"Could not find item  {id.UID} in cache");
        
        return found;
    }

    public void Store(IIdentifier id, T component)
    {
        // Only add if not already present
        if (id.IsEmpty()) return;
        if (CachedComponents.TryAdd(id, component))
        {
            _logger.Log("BaseComponentCache", $"Adding component {component.Name}  to {ComponentType} cache ");

        }
        
        else  _logger.LogWarning("BaseComponentCache", $"Adding component {component.Name}  to {ComponentType} cache Failed - Element already exist in cache");

    }
    
    public void StoreOrReplace(IIdentifier id, T component)
    {
        if (id.IsEmpty()) return;
        // Add or replace regardless
        _logger.Log("BaseComponentCache", $"Updated component {component.Name}  in {ComponentType} cache ");

        CachedComponents[id] = component;
    }

    public void Remove(IIdentifier id)
    {
        //doesnt exist in cache, or id is empty
        if (!TryGet(id, out var value))
        {
            _logger.LogWarning("BaseComponentCache", $"Removing component {id.UID}  from {ComponentType} cache Failed");
            return;
        }
        
        CachedComponents.Remove(id);
        _logger.Log("BaseComponentCache", $"Removing component {id.UID}  from {ComponentType} cache");

        
    }

    public void Clear()
    {
        _logger.Log("BaseComponentCache", $"Cleared all {ComponentType} cache");
        CachedComponents.Clear();
    }

    
}