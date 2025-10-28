using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Models.DataManagement.DB.Model.Handlers;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB;

//bridge to do local database
public class DataRepository : ILocalRepository
{
    
    private readonly ILogger _workoutLogger;
    private readonly GainLabSQLDBContext _sqldbContext;
    private Dictionary<EntityType, IDBHandler> _handlers = new();
    
    
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

    public Task<ResultList<TEntity>> SaveComponentsAsync<TId, TEntity>(EntityType componentType, List<TEntity> list)
    {
        throw new NotImplementedException();
    }


    public async Task<Result<Dictionary<EntityType, ResultList<TEntity>>>> BatchSaveComponentsAsync<TEntity>(Dictionary<EntityType, ResultList<TEntity>> fileData)
    {
        //todo
        return new Result<Dictionary<EntityType, ResultList<TEntity>>>(true, new(), "none");
    }

    public async Task<Result<Dictionary<EntityType, ResultList<TEntity>>>> GetAllComponentsAsync<TEntity>()
    {
        //todo
        return new Result<Dictionary<EntityType, ResultList<TEntity>>>(true, new(), "none");

    }

    public Task<ResultList<TEntity>> GetAllComponentsOfTypeAsync<TEntity>()
    {
        throw new NotImplementedException();
    }

    public Task<Result<TEntity>> SaveComponentAsync<TEntity>(TEntity component)
    {
        throw new NotImplementedException();
    }

    public Task<Dictionary<EntityType, ResultList<IEntity>>> GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public Task MarkDeletedAsync(EntityType type, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    public async Task<ISyncState> GetSyncStateAsync(string partition)
    {
        _workoutLogger.Log(nameof(DataRepository), "Get Sync State");
        
        if (string.IsNullOrWhiteSpace(partition))
            partition = "global";

        var row = await _sqldbContext.SyncStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Partition == partition);
        
        _workoutLogger.Log(nameof(DataRepository), $"Sync State : {(row != null? row.Partition: "none")}");

        
        return row ?? new SyncState
        {
            Partition   = partition,
            SeedVersion = 1,
            CursorsJson = "{}"
        };
    }

    public async Task SaveSyncStateAsync(ISyncState state)
    {
        if (state is null) throw new ArgumentNullException(nameof(state));

        if(state is not SyncState syncState) throw new ArgumentNullException(nameof(state));
        
        _workoutLogger.Log(nameof(DataRepository), $"Save Sync State ");

        
        var existing = await _sqldbContext.SyncStates
            .FirstOrDefaultAsync(x => x.Partition == syncState.Partition);

        if (existing is null)
        {
            var toAdd = SyncState.From(syncState);
            _sqldbContext.SyncStates.Add(toAdd);
        }
        else
        {
            SyncState.Copy(syncState, existing);
            _sqldbContext.SyncStates.Update(existing);
        }

        await _sqldbContext.SaveChangesAsync();
    }


    private void CreateHandlers()
    {
        _handlers = new();
        //todo for each EntityType 
        _handlers[EntityType.Equipment] = new EquipmentIdbHandler(_sqldbContext, _workoutLogger);
 
    }
    
}