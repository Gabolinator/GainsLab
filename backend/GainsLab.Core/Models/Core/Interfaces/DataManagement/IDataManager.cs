using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement;

public interface IDataManager
{

    Task InitializeAsync();

    Task<Result> LoadAndCacheDataAsync();
    
    Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;

    Task<ResultList<T>> TryGetComponentsAsync<T>(IEnumerable<IIdentifier> ids) where T : IWorkoutComponent;

   
    Task<Result> SaveComponentAsync<T>(T component) where T : IWorkoutComponent;
    Task<ResultList> SaveComponentsAsync<T>(IEnumerable<T> components) where T : IWorkoutComponent;

    Task<ResultList<T>> TryResolveComponentsAsync<T>(List<IIdentifier> toResolve) where T : IWorkoutComponent;
    Task<Result<TComponent>> TryResolveComponentAsync<TComponent>(IIdentifier unresolved) where TComponent : IWorkoutComponent;
    Task<Result> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;

    Task<Result> SaveAllDataToFilesAsync();
}