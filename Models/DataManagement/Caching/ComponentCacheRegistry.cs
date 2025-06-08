using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.Utilities;

namespace GainsLab.Models.DataManagement.Caching;

public class ComponentCacheRegistry : IComponentCacheRegistry
{
    
    private Dictionary<eWorkoutComponents, IComponentCacheBase> _caches = new ();
    
    public async Task InitializeAsync()
    {
        //create all components cache
        _caches[eWorkoutComponents.Equipment] = new EquipmentsCache();
        _caches[eWorkoutComponents.EquipmentList] = new EquipmentListCache();
        _caches[eWorkoutComponents.Muscle] = new MusclesCache();
        _caches[eWorkoutComponents.MuscleGroup] = new MusclesGroupCache();
        _caches[eWorkoutComponents.MovementCategory] = new MovementCategoryCache();
        _caches[eWorkoutComponents.Movement] = new MovementCache();
        _caches[eWorkoutComponents.WorkloadProfile] = new WorkloadProfileCache();
        
    }

    public bool TryGetCache<T>(out IComponentCache<T> componentCache) where T : IWorkoutComponent
    {

        var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
        
        if (_caches.TryGetValue(type, out var baseCache) &&
            baseCache is IComponentCache<T> typedCache)
        {
            componentCache = typedCache;
            return true;
        }

        componentCache = default!;
        return false;
    }

  public bool TryGetComponent<T>(IIdentifier id, out T? component) where T : IWorkoutComponent
    {
        component = default;

        if (TryGetCache<T>(out var cache))
        {
            return cache.TryGet(id, out component);
        }

        return false;
    }

    public bool TryGetComponents<T>(List<IIdentifier> ids, out ComponentLists<T>? result) where T : IWorkoutComponent
    {
        result = new ComponentLists<T>();
       
        if (!TryGetCache<T>(out var cache))
            return false;

        foreach (var id in ids)
        {
            if (cache.TryGet(id, out var component) && component != null)
            {
                result.AddComponent(component);
            }
            
        }

        return true;
    }

    public void Store<T>(T component) where T : IWorkoutComponent
    {
        var id = component.Identifier;
        if(id.IsEmpty()) return;
        
        if (TryGetCache<T>(out var cache))
        {
            cache.Store(id, component);
        }
    }

    public void StoreAll<T>(List<T> components) where T : IWorkoutComponent
    {
        if (!TryGetCache<T>(out var cache))
            return;

        var ids = components
            .Select(it => it.Identifier)
            .Where(id => !id.IsEmpty())
            .ToList();
        
        for (int i = 0; i < Math.Min(ids.Count, components.Count); i++)
        {
            cache.Store(ids[i], components[i]);
        }
    }
    
    public void StoreAll<T>(eWorkoutComponents type, List<T>  components) where T : IWorkoutComponent
    {
        if (!_caches.TryGetValue(type, out var baseCache) ||
            baseCache is not IComponentCache<T> cache)
        {
            return;
        }

        var ids = components.Select(it => it.Identifier).ToList();

        for (int i = 0; i < Math.Min(ids.Count, components.Count); i++)
        {
            cache.Store(ids[i], components[i]);
        }
    }

    public void Remove<T>(IIdentifier id) where T : IWorkoutComponent
    {
        if (TryGetCache<T>(out var cache))
        {
            cache.Remove(id);
        }
    }

    public void RemoveAll<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        if (TryGetCache<T>(out var cache))
        {
            foreach (var id in ids)
            {
                cache.Remove(id);
            }
        }
    }

    public void ClearCache<T>() where T : IWorkoutComponent
    {
        if (TryGetCache<T>(out var cache))
        {
            cache.Clear();
        }
    }

    public void ClearAllCaches()
    {
        foreach (var cache in _caches.Values)
        {
            cache.Clear();
        }
    }
}