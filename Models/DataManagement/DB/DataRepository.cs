using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Results;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;


namespace GainsLab.Models.DataManagement.DB;

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
        _workoutLogger.Log(nameof(DataRepository), "Ensuring database exists...");
        await _dbContext.Database.EnsureCreatedAsync();
        _workoutLogger.Log(nameof(DataRepository), "Database ready.");
        
        
    }

    public async Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        //todo
        return ResultsFactory.Failure<T>("Not implemented");
    }

    public async Task<List<Result<T>>> TryGetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        return new()
        {
            ResultsFactory.Failure<T>("Not implemented")
        };
    }

    public async Task DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        //todo
    }

    public async Task SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent
    {
        //todo
    }

    public async Task BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> fileData)
    {
        //todo
    }

    public async Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> GetAllComponentsAsync()
    {
        //todo
        return new();
    }

    public async Task<List<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type)
    {
        //todo
        return new();
    }

    public async Task SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    {
        //todo
    }
}