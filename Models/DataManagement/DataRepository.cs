using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.Logging;


namespace GainsLab.Models.DataManagement;

//bridge to do database
public class DataRepository : IDataProvider
{
    
    private readonly IWorkoutLogger _workoutLogger;
    private readonly GainLabDBContext _dbContext; 

    public DataRepository(IWorkoutLogger workoutLogger, GainLabDBContext dbContext)
    {
        _workoutLogger = workoutLogger;
        _dbContext = dbContext;
    }
    
    public async Task InitializeAsync()
    {
        _workoutLogger.Log(nameof(DataRepository), "Loading Data started");
        await Task.Delay(500);
        _workoutLogger.Log(nameof(DataRepository), "Loading Data finished");
    }

    public Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task<List<Result<T>>> TryGetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }

    public Task BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> fileData)
    {
        throw new System.NotImplementedException();
    }

    public Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> GetAllComponentsAsync()
    {
        throw new System.NotImplementedException();
    }

    public Task<List<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type)
    {
        throw new System.NotImplementedException();
    }

    public Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    {
        throw new System.NotImplementedException();
    }
}