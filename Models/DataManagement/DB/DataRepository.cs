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
    
    private readonly IWorkoutLogger _workoutLogger;
    private readonly GainLabDBContext _dbContext;
    private Dictionary<eWorkoutComponents, IDBHandler> _handlers = new();

    public DataRepository(IWorkoutLogger workoutLogger, GainLabDBContext dbContext)
    {
        _workoutLogger = workoutLogger;
        _dbContext = dbContext;
    }
    
    public async Task<Result<bool>> InitializeAsync()
    {

        try
        {
            _workoutLogger.Log(nameof(DataRepository), "Loading Data started");
            _workoutLogger.Log(nameof(DataRepository), "Ensuring database exists...");
            await _dbContext.Database.MigrateAsync();
      
            //await _dbContext.Database.EnsureCreatedAsync();
            _workoutLogger.Log(nameof(DataRepository), "Database ready.");
            CreateHandlers();

            return Results.SuccessResult();
        }
        catch (Exception e)
        {
            _workoutLogger.LogError(nameof(DataRepository), $"Error ininitalizing Data repository {e}");
            return Results.FailureResult($"Error ininitalizing Data repository {e}");
        }
      
        

    }

    private void CreateHandlers()
    {
        _handlers = new();
        //todo for each eWorkoutComponents 
        _handlers[eWorkoutComponents.Equipment] = new EquipmentIdbHandler(_dbContext, _workoutLogger);
 
    }

    public async Task<Result<T>> TryGetComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        //todo
        return Results.FailureResult<T>("Not implemented");
    }

    public async Task<List<Result<T>>> TryGetComponentsAsync<T>(List<IIdentifier> ids) where T : IWorkoutComponent
    {
        return new()
        {
            Results.FailureResult<T>("Not implemented")
        };
    }

    public async Task<Result<bool>> DeleteComponentAsync<T>(IIdentifier id) where T : IWorkoutComponent
    {
        //todo
        return Results.NotImplementedResult<bool>("DeleteComponentAsync");
    }

    public async Task<Result<List<T>>> SaveComponentsAsync<T>(eWorkoutComponents componentType, List<T> list) where T : IWorkoutComponent
    {
        //todo
        return Results.NotImplementedResult<List<T>>("SaveComponentsAsync");
    }

    public async Task<Result<Dictionary<eWorkoutComponents, List<Result<IWorkoutComponent>>>>>  BatchSaveComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> fileData)
    {
        return Results.NotImplementedResult<Dictionary<eWorkoutComponents, List<Result<IWorkoutComponent>>>>("BatchSaveComponentsAsync");

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

    public async Task<Result<T>> SaveComponentAsync<T>(T component) where T : IWorkoutComponent
    {
       
        if(_handlers.Count ==0) return Results.FailureResult<T>("Failed to Save components. No Db handlers.");
        
        if(!_handlers.TryGetValue(component.ComponentType, out var handler)) return Results.FailureResult<T>($"Failed to Save component. No Db handler for type {component.ComponentType} found.");
        
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
            ? Results.SuccessResult<T>(component)
            : Results.FailureResult<T>($"Failed to add {dto.Iuid} to data base : {result.ErrorMessage}");

    }
}