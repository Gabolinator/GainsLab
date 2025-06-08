using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.Caching.Interface
{
    public interface IComponentCacheRegistry
    {
        public Task InitializeAsync();
        
        bool TryGetCache<T>(out  IComponentCache<T> componentCache) where T : IWorkoutComponent;
        
        bool TryGetComponent<T>(IIdentifier id, out T? component)where T : IWorkoutComponent;
        bool TryGetComponents<T>(List<IIdentifier> ids, out ComponentLists<T>? component) where T : IWorkoutComponent;
        
        void Store<T>(IIdentifier id, T component)where T : IWorkoutComponent;
        void StoreAll<T>(List<IIdentifier> ids, List<T> components) where T : IWorkoutComponent;
        
        void Remove<T>(IIdentifier id) where T : IWorkoutComponent;
        void RemoveAll<T>(List<IIdentifier> ids) where T : IWorkoutComponent;
       
        void ClearCache<T>()where T : IWorkoutComponent;
        void ClearAllCaches();
    }
}