using System.Collections.Generic;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching.Interface;

namespace GainsLab.Models.DataManagement.Caching;

public abstract class BaseComponentCache<T> : IComponentCache<T> where T : IWorkoutComponent
{
    
    public abstract eWorkoutComponents ComponentType { get; }
    public Dictionary<IIdentifier, T> CachedComponents { get; set; } = new();

    public bool TryGet(IIdentifier id, out T? component)
    {
        if (id.IsEmpty())
        {
            component = default;
            return false;
        }

        return CachedComponents.TryGetValue(id, out component);
    }

    public void Store(IIdentifier id, T component)
    {
        // Only add if not already present
        if (id.IsEmpty()) return;
        CachedComponents.TryAdd(id,component);
    }
    
    public void StoreOrReplace(IIdentifier id, T component)
    {
        if (id.IsEmpty()) return;
        // Add or replace regardless
        CachedComponents[id] = component;
    }

    public void Remove(IIdentifier id)
    {
        //doesnt exist in cache, or id is empty
        if (!TryGet(id, out var value))
        {
            return;
        }
        
        CachedComponents.Remove(id);
        
    }

    public void Clear()
    {
        CachedComponents.Clear();
    }

    
}