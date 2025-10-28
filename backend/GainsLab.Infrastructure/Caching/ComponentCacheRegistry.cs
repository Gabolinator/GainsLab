using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.Caching;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.Caching;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.DataManagement.Caching.Interface;

namespace GainsLab.Models.DataManagement.Caching;

public class ComponentCacheRegistry : IComponentCacheRegistry
{

    private readonly ILogger _logger;
    private Dictionary<EntityType, IComponentCacheBase> _caches = new ();

    public ComponentCacheRegistry(ILogger logger)
    {
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        //create all components cache
        _caches[EntityType.Equipment] = new EquipmentsCache(_logger);
      //  _caches[eWorkoutComponents.EquipmentList] = new EquipmentListCache(_logger);
        _caches[EntityType.Muscle] = new MusclesCache(_logger);
        //_caches[eWorkoutComponents.MuscleGroup] = new MusclesGroupCache(_logger);
        _caches[EntityType.MovementCategory] = new MovementCategoryCache(_logger);
        _caches[EntityType.Movement] = new MovementCache(_logger);
       // _caches[eWorkoutComponents.WorkloadProfile] = new WorkloadProfileCache(_logger);
        
    }

    public bool TryGetCache<TId, TEntity>(out IComponentCache<TId, TEntity> componentCache)
    {
        throw new NotImplementedException();
    }

    public bool TryGetComponent<TId, TEntity>(TId id, out TEntity? component)
    {
        throw new NotImplementedException();
    }

    public bool TryGetComponents<TId, TEntity>(List<TId> ids, out List<TEntity>? components)
    {
        throw new NotImplementedException();
    }

    public void Store<TEntity>(TEntity component)
    {
        throw new NotImplementedException();
    }

    public void StoreAll<TEntity>(List<TEntity> components)
    {
        throw new NotImplementedException();
    }

    public void StoreAll<TEntity>(EntityType type, List<TEntity> components)
    {
        throw new NotImplementedException();
    }

    public void Remove<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    public void RemoveAll<TId, TEntity>(List<TId> ids)
    {
        throw new NotImplementedException();
    }

    public void ClearCache<TEntity>()
    {
        throw new NotImplementedException();
    }

    public void ClearAllCaches()
    {
        throw new NotImplementedException();
    }

    //   public bool TryGetCache<T>(out IComponentCache<T> componentCache) where T : IWorkoutComponent
  //   {
  //
  //       var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
  //       
  //       if (_caches.TryGetValue(type, out var baseCache) &&
  //           baseCache is IComponentCache<T> typedCache)
  //       {
  //           componentCache = typedCache;
  //           return true;
  //       }
  //      
  //       _logger.LogWarning(nameof(ComponentCacheRegistry), $"Could not find cache for type {type} ");
  //
  //       componentCache = default!;
  //       return false;
  //   }
  //
  // public bool TryGetComponent<T>(IIdentifier id, out T? component) where T : IWorkoutComponent
  //   {
  //       component = default;
  //
  //       if (TryGetCache<T>(out var cache))
  //       {
  //           return cache.TryGet(id, out component);
  //       }
  //
  //       return false;
  //   }
  //
  //   public bool TryGetComponents<T>(List<IIdentifier> ids, out ComponentLists<T>? result) where T : IWorkoutComponent
  //   {
  //       result = new ComponentLists<T>();
  //      
  //       if (!TryGetCache<T>(out var cache))
  //           return false;
  //
  //       foreach (var id in ids)
  //       {
  //           if (cache.TryGet(id, out var component) && component != null)
  //           {
  //               result.AddComponent(component);
  //           }
  //           
  //       }
  //
  //       return true;
  //   }
  //
  //   public void Store<T>(T component) where T : IWorkoutComponent
  //   {
  //       var id = component.Identifier;
  //       if(id.IsEmpty()) return;
  //       
  //       
  //       if (TryGetCache<T>(out var cache))
  //       {
  //           _logger.Log(nameof(ComponentCacheRegistry), $"Adding component {component.Name}  to {cache.ComponentType} cache ");
  //           cache.Store(id, component);
  //       }
  //   }
  //
  //   public void StoreAll<T>(List<T> components) where T : IWorkoutComponent
  //   {
  //       if (!TryGetCache<T>(out var cache))
  //           return;
  //
  //       var comps = components
  //           .Where(comp => !comp.Identifier.IsEmpty())
  //           .ToList();
  //
  //       foreach (var comp in comps)
  //       {
  //           cache.Store(comp.Identifier,comp );
  //       }
  //       
  //   }
  //   
  //   public void StoreAll<T>(eWorkoutComponents type, List<T>  components) where T : IWorkoutComponent
  //   {
  //       if (!_caches.TryGetValue(type, out var baseCache) ||
  //           baseCache is not IComponentCache<T> cache)
  //       {
  //           return;
  //       }
  //
  //       var ids = components.Select(it => it.Identifier).ToList();
  //
  //       for (int i = 0; i < Math.Min(ids.Count, components.Count); i++)
  //       {
  //           cache.Store(ids[i], components[i]);
  //       }
  //   }
  //
  //   public void Remove<T>(IIdentifier id) where T : IWorkoutComponent
  //   {
  //       if (TryGetCache<T>(out var cache))
  //       {
  //           cache.Remove(id);
  //       }
  //   }
  //
  //   public void RemoveAll<T>(List<IIdentifier> ids) where T : IWorkoutComponent
  //   {
  //       if (TryGetCache<T>(out var cache))
  //       {
  //           foreach (var id in ids)
  //           {
  //               cache.Remove(id);
  //           }
  //       }
  //   }
  //
  //   public void ClearCache<T>() where T : IWorkoutComponent
  //   {
  //       if (TryGetCache<T>(out var cache))
  //       {
  //           cache.Clear();
  //       }
  //   }
  //
  //   public void ClearAllCaches()
  //   {
  //       foreach (var cache in _caches.Values)
  //       {
  //           cache.Clear();
  //       }
  //   }
}