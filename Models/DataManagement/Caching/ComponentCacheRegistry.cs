using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching.Interface;

namespace GainsLab.Models.DataManagement.Caching;

public class ComponentCacheRegistry : IComponentCacheRegistry
{
    public async Task InitializeAsync()
    {
        
    }

    public bool TryGetCache<T>(out IComponentCache<T> componentCache) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetComponent<T>(IIdentifier id, out T? component) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public bool TryGetComponents<T>(List<IIdentifier> ids, out ComponentLists<T>? component) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public void Store<T>(IIdentifier id, T component) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public void StoreAll<T>(List<IIdentifier> ids, List<T> components) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public void Remove<T>(IIdentifier id) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public void RemoveAll<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public void ClearCache<T>() where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public void ClearAllCaches()
    {
        throw new System.NotImplementedException();
    }
}