using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement.DB;

public interface IDataProvider
{
    Task<Result> InitializeAsync();
    Task<Result<T>> GetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task<ResultList<T>> GetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent;
    
    Task<Result> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task<ResultList<T>> SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent;
    Task<Result<Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>>>> BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>> fileData);
    Task<Result<Dictionary<eWorkoutComponents,ResultList<IWorkoutComponent>>>> GetAllComponentsAsync();
    Task<ResultList<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type);
    
    Task<Result<T>> SaveComponentAsync<T>(T component) where T : IWorkoutComponent;
}