using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Handlers;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB;
using GainsLab.Models.DataManagement.DB.Model.Handlers;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB;

//bridge to do database
public class DataRepository : IDataProvider
{
    
    private readonly ILogger _workoutLogger;
    private readonly GainLabSQLDBContext _sqldbContext;
    private Dictionary<eWorkoutComponents, IDBHandler> _handlers = new();

    public DataRepository(ILogger workoutLogger, GainLabSQLDBContext sqldbContext)
    {
        _workoutLogger = workoutLogger;
        _sqldbContext = sqldbContext;
    }
    
    public async Task<Result> InitializeAsync()
    {

        try
        {
            _workoutLogger.Log(nameof(DataRepository), "Loading Data started");
            _workoutLogger.Log(nameof(DataRepository), "Ensuring database exists...");
            await _sqldbContext.Database.MigrateAsync();
            
            _workoutLogger.Log(nameof(DataRepository), "Database ready.");
            CreateHandlers();

            return Result.SuccessResult();
        }
        catch (Exception e)
        {
            _workoutLogger.LogError(nameof(DataRepository), $"Error ininitalizing Data repository {e}");
            return Result.Failure($"Error initializing Data repository {e}");
        }
      
        

    }

    public Task<Result<TEntity>> GetComponentAsync<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<ResultList<TEntity>> GetComponentsAsync<TId, TEntity>(List<TId> ids)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteComponentAsync<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<ResultList<TEntity>> SaveComponentsAsync<TId, TEntity>(eWorkoutComponents componentType, List<TEntity> list)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Dictionary<eWorkoutComponents, ResultList<TEntity>>>> BatchSaveComponentsAsync<TEntity>(Dictionary<eWorkoutComponents, ResultList<TEntity>> fileData)
    {
        throw new NotImplementedException();
    }

    public Task<Result<Dictionary<eWorkoutComponents, ResultList<TEntity>>>> GetAllComponentsAsync<TEntity>()
    {
        throw new NotImplementedException();
    }

    public Task<ResultList<TEntity>> GetAllComponentsOfTypeAsync<TEntity>()
    {
        throw new NotImplementedException();
    }

    public Task<Result<TEntity>> SaveComponentAsync<TEntity>(TEntity component)
    {
        throw new NotImplementedException();
    }

    private void CreateHandlers()
    {
        _handlers = new();
        //todo for each eWorkoutComponents 
        _handlers[eWorkoutComponents.Equipment] = new EquipmentIdbHandler(_sqldbContext, _workoutLogger);
 
    }

    // public async Task<Result<T>> GetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    // {
    //     
    //     return Results.NotImplementedResult<T>();
    // }
    //
    // public async Task<ResultList<T>> GetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    // {
    //     return Results.NotImplementedResults<T>("Not implemented : GetComponentsAsync");
    // }
    //
    //
    // public async Task<Result> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    // {
    //     //todo
    //     return Results.NotImplementedResult<bool>("DeleteComponentAsync");
    // }
    //
    // public async Task<ResultList<T>> SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent
    // {
    //     return Results.NotImplementedResults<T>("SaveComponentsAsync");
    // }
    //
    // public async Task<Result<Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>>>> BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>> fileData)
    // {
    //     return Results.NotImplementedResult<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>>("Not yet implemented : BatchSaveComponentsAsync");
    //
    // }
    //
    //
    // public async Task<Result<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>>> GetAllComponentsAsync()
    // {
    //     return Results.NotImplementedResult<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>>("Not yet implemented");
    // }
    //
    //
    //
    // public async Task<ResultList<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type)
    // {
    //     //todo
    //     return Results.NotImplementedResults<IWorkoutComponent>("Not yet implemented");
    // }
    //
    // public async Task<Result<T>> SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    // {
    //    
    //     if(_handlers.Count ==0) return Result<T>.Failure("Failed to Save components. No Db handlers.");
    //     
    //     if(!_handlers.TryGetValue(component.ComponentType, out var handler)) return Result<T>.Failure($"Failed to Save component. No Db handler for type {component.ComponentType} found.");
    //     
    //     _workoutLogger.Log("DataRepository",$"Saving component {component.Identifier.Slug} to data base");
    //     IDto dto = component.ToDTO();
    //     
    //     
    //     var result= await handler.AddOrUpdateAsync(dto);
    //
    //
    //     if (result.Success)
    //     {
    //         if (result.Value != null)
    //         {
    //             component.Identifier.DbID = result.Value.Iid;
    //             if(result.Value!.Iid <=0)  _workoutLogger.LogWarning("DataRepository",$"Updated negative id to db : {dto.Iuid} with id {result.Value.Iid}");
    //
    //         }
    //        
    //         _workoutLogger.Log("DataRepository",$"Successfully added {dto.Iuid} to data base with id {component.Identifier.DbID}");
    //     }
    //     else  _workoutLogger.LogWarning("DataRepository",$" Failed to add {dto.Iuid} to data base : {result.ErrorMessage}");
    //
    //    
    //     return result.Success
    //         ? Result<T>.SuccessResult(component)
    //         : Result<T>.Failure($"Failed to add {dto.Iuid} to data base : {result.ErrorMessage}");
    //
    // }
}