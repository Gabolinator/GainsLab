using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement;

public interface IDataManager
{

    Task InitializeAsync();

    Task LoadAndCacheDataAsync();
    
    Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;

    Task<Result<IEnumerable<T>>> TryGetComponentsAsync<T>(IEnumerable<IIdentifier> ids) where T : IWorkoutComponent;

   
    Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent;
    Task SaveComponentsAsync<T>(IEnumerable<T> components) where T : IWorkoutComponent;

    Task<Result<IEnumerable<T>>> TryResolveComponentsAsync<T>(List<IIdentifier> toResolve) where T : IWorkoutComponent;
    Task<Result<TComponent>> TryResolveComponentAsync<TComponent>(IIdentifier unresolved) where TComponent : IWorkoutComponent;
    Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    
}