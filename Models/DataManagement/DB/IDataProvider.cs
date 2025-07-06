using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement.DB;

public interface IDataProvider
{
    Task<Result<bool>> InitializeAsync();
    Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task<List<Result<T>>> TryGetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent;
    
    Task<Result<bool>> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task<Result<List<T>>> SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent;
    Task<Result<Dictionary<eWorkoutComponents, List<Result<IWorkoutComponent>>>>>  BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> fileData);
    Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> GetAllComponentsAsync();
    Task<List<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type);
    
    Task<Result<T>> SaveComponentAsync<T>(T component) where T : IWorkoutComponent;
}