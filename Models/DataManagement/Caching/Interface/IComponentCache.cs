using System.Collections.Generic;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.DataManagement.Caching.Interface;

public interface IComponentCache<T> : IComponentCacheBase where T : IWorkoutComponent
{
    Dictionary<IIdentifier, T> CachedComponents { get; set; }
    bool TryGet(IIdentifier id, out T? component);
    void Store(IIdentifier id, T component);
    
    void StoreOrReplace(IIdentifier id, T component);
    void Remove(IIdentifier id);
    
}