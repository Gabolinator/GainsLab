using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.DataManagement.DB.Model.Handlers;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;


namespace GainsLab.Models.DataManagement.DB;

//bridge to do database
public class DataRepository : IDataProvider
{
    
    private readonly ILogger _workoutLogger;
    private readonly GainLabDBContext _dbContext;
    private Dictionary<eWorkoutComponents, IDBHandler> _handlers = new();

    public DataRepository(ILogger workoutLogger, GainLabDBContext dbContext)
    {
        _workoutLogger = workoutLogger;
        _dbContext = dbContext;
    }
    
    public async Task<Result> InitializeAsync()
    {

        try
        {
            _workoutLogger.Log(nameof(DataRepository), "Loading Data started");
            _workoutLogger.Log(nameof(DataRepository), "Ensuring database exists...");
            await _dbContext.Database.MigrateAsync();
      
            //await _dbContext.Database.EnsureCreatedAsync();
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

    private void CreateHandlers()
    {
        _handlers = new();
        //todo for each eWorkoutComponents 
        _handlers[eWorkoutComponents.Equipment] = new EquipmentIdbHandler(_dbContext, _workoutLogger);
 
    }

    public async Task<Result<T>> GetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        
        return Results.NotImplementedResult<T>();
    }

    public async Task<ResultList<T>> GetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        return Results.NotImplementedResults<T>("Not implemented : GetComponentsAsync");
    }
    

    public async Task<Result> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        //todo
        return Results.NotImplementedResult<bool>("DeleteComponentAsync");
    }

    public async Task<ResultList<T>> SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent
    {
        return Results.NotImplementedResults<T>("SaveComponentsAsync");
    }

    public async Task<Result<Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>>>> BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>> fileData)
    {
        return Results.NotImplementedResult<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>>("Not yet implemented : BatchSaveComponentsAsync");

    }
    

    public async Task<Result<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>>> GetAllComponentsAsync()
    {
        return Results.NotImplementedResult<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>>("Not yet implemented");
    }



    public async Task<ResultList<IWorkoutComponent>> GetAllComponentsOfTypeAsync(eWorkoutComponents type)
    {
        //todo
        return Results.NotImplementedResults<IWorkoutComponent>("Not yet implemented");
    }

    public async Task<Result<T>> SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    {
       
        if(_handlers.Count ==0) return Result<T>.Failure("Failed to Save components. No Db handlers.");
        
        if(!_handlers.TryGetValue(component.ComponentType, out var handler)) return Result<T>.Failure($"Failed to Save component. No Db handler for type {component.ComponentType} found.");
        
        _workoutLogger.Log("DataRepository",$"Saving component {component.Identifier.UID} to data base");
        IDto dto = component.ToDTO();
        
        
        var result= await handler.AddOrUpdateAsync(dto);


        if (result.Success)
        {
            if (result.Value != null)
            {
                component.Identifier.ID = result.Value.Iid;
                if(result.Value!.Iid <=0)  _workoutLogger.LogWarning("DataRepository",$"Updated negative id to db : {dto.Iuid} with id {result.Value.Iid}");

            }
           
            _workoutLogger.Log("DataRepository",$"Successfully added {dto.Iuid} to data base with id {component.Identifier.ID}");
        }
        else  _workoutLogger.LogWarning("DataRepository",$" Failed to add {dto.Iuid} to data base : {result.ErrorMessage}");

       
        return result.Success
            ? Result<T>.SuccessResult(component)
            : Result<T>.Failure($"Failed to add {dto.Iuid} to data base : {result.ErrorMessage}");

    }
}