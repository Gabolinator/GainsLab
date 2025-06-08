using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement;

public interface IDataManager
{

    Task InitializeAsync();

    Task LoadAndCacheDataAsync();
    
    Task<T?> GetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task<List<T>> GetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent;

    Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent;
    Task SaveComponentsAsync<T>(IEnumerable<T> components) where T : IWorkoutComponent;

   Task<IEnumerable<TComponent>> ResolveComponentsAsync<TComponent>(List<IIdentifier> unresolved) where TComponent : IWorkoutComponent;

   Task<TComponent> ResolveComponentAsync<TComponent>(IIdentifier unresolved) where TComponent : IWorkoutComponent;

    Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    
}