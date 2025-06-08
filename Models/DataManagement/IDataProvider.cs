using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement;

public interface IDataProvider
{
    Task InitializeAsync();
    Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task<List<Result<T>>> TryGetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent;
    
    Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent;
    Task SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent;
    Task BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> fileData);
    Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> GetAllComponentsAsync();
    Task<List<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type);


    Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent;
}