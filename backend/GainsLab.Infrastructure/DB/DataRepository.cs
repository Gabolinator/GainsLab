using System.Diagnostics;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Handlers;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB;

/// <summary>
/// SQLite-backed repository that persists domain entities locally for the desktop application.
/// </summary>
public class DataRepository : ILocalRepository
{
    
    private readonly ILogger _logger;
    private readonly GainLabSQLDBContext _sqldbContext;
    private Dictionary<EntityType, IDBHandler> _handlers = new();
    
    
    /// <summary>
    /// Initializes a new instance of the <see cref="DataRepository"/> class.
    /// </summary>
    /// <param name="logger">Logger used to emit diagnostic messages.</param>
    /// <param name="sqldbContext">EF Core context used to access the local database.</param>
    public DataRepository(ILogger logger, GainLabSQLDBContext sqldbContext)
    {
        _logger = logger;
        _sqldbContext = sqldbContext;
    }
    
    /// <inheritdoc />
    public async Task<Result> InitializeAsync()
    {

        try
        {
            _logger.Log(nameof(DataRepository), "Loading Data started");
            _logger.Log(nameof(DataRepository), "Ensuring database exists...");
            await _sqldbContext.Database.MigrateAsync();
            
            _logger.Log(nameof(DataRepository), "Database ready.");
            CreateHandlers();

            return Result.SuccessResult();
        }
        catch (Exception e)
        {
            _logger.LogError(nameof(DataRepository), $"Error ininitalizing Data repository {e}");
            return Result.Failure($"Error initializing Data repository {e}");
        }
      
        

    }

    public Task<Result<TEntity>> GetComponentAsync<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> GetComponentsAsync<TId, TEntity>(List<TId> ids)
    {
        throw new NotImplementedException();
    }

    public Task<Result> DeleteComponentAsync<TId, TEntity>(TId id)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TEntity>> SaveComponentsAsync<TId, TEntity>(EntityType componentType, List<TEntity> list)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>> BatchSaveComponentsAsync(
    Dictionary<EntityType, IReadOnlyList<IEntity>> entities,
    CancellationToken ct = default)
{
    if (entities is null || entities.Count == 0)
        return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure("No entities to save.");

    // Make sure all handlers exist up front (fail fast)
    var missingHandlers = entities.Keys.Where(k => !_handlers.ContainsKey(k)).ToList();
    
    if (missingHandlers.Count > 0)
    {
        foreach (var t in missingHandlers)
            _logger.LogWarning(nameof(DataRepository), $"No handler registered for {t}.");
        return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure(
            $"Missing handlers for: {string.Join(", ", missingHandlers)}");
    }

    var successes = new Dictionary<EntityType, IReadOnlyList<IEntity>>(entities.Count);
    var errors = new List<string>();

    await using var tx = await _sqldbContext.Database.BeginTransactionAsync(ct);
    try
    {
        foreach (var kvp in entities)
        {
            var type = kvp.Key;
            var items = kvp.Value;

           
            var dtos = new List<IDto>(items.Count);
            foreach (var e in items)
            {
                var dto = e.ToDTO();
                if (dto is null)
                {
                    var msg = $"Mapping to DTO returned null for entity type {type} .";
                    _logger.LogWarning(nameof(DataRepository), msg);
                    errors.Add(msg);
                    continue; // skip this item; you can choose to fail the whole batch instead
                }
                dtos.Add(dto);
            }
            
            if (dtos.Count == 0)
            {
                errors.Add($"No valid DTOs produced for type {type}.");
                continue;
            }

           
            var handler = _handlers[type];
            Result<IReadOnlyList<IDto>> handlerResult;
            try
            {
                //we add or update but dont let the handler save
                handlerResult = await handler.AddOrUpdateAsync(dtos, save : false ,ct);
            }
            catch (Exception ex)
            {
                var baseMsg = ex.GetBaseException().Message;
                _logger.LogError(nameof(DataRepository), $"Handler for {type} threw: {baseMsg}");
                errors.Add($"Handler for {type} failed: {baseMsg}");
                continue;
            }

            if (!handlerResult.Success || handlerResult.Value is null)
            {
                var msg = $"Handler for {type} returned failure: {handlerResult.ErrorMessage ?? "unknown error"}.";
                _logger.LogWarning(nameof(DataRepository), msg);
                errors.Add(msg);
                continue;
            }
            
            var saved = new List<IEntity>(handlerResult.Value.Count);
            foreach (var savedDto in handlerResult.Value)
            {
                var domain = savedDto.ToDomain();
                if (domain is null)
                {
                    var msg = $"DTO->Domain mapping returned null for type {type} (DTO: {savedDto.GetType().Name}).";
                    _logger.LogWarning(nameof(DataRepository), msg);
                    errors.Add(msg);
                    continue;
                }
                saved.Add(domain);
            }

            if (saved.Count == 0)
            {
                errors.Add($"No domain entities produced after save for type {type}.");
                continue;
            }

            successes[type] = saved;
        }
        
        
        if (successes.Count == entities.Count && errors.Count == 0)
        {
            await _sqldbContext.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);
            return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.SuccessResult(successes);
        }
        else
        {
            // All-or-nothing: rollback if any type failed
            await tx.RollbackAsync(ct);

            var reason = errors.Count == 0
                ? "Unknown error while saving entities."
                : string.Join(" | ", errors);

            return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure(reason);
        }
    }
    catch (Exception ex)
    {
        await tx.RollbackAsync(ct);
        var msg = ex.GetBaseException().Message;
        _logger.LogError(nameof(DataRepository), $"Batch save failed: {msg}");
        return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure($"Batch save failed: {msg}");
    }
    
}

    /// <inheritdoc />
    public async Task<Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>> GetAllComponentsAsync()
    {
        if (!_handlers.Any()) return Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure("No handlers");

        var dict = new Dictionary<EntityType, IReadOnlyList<IEntity>>();
        foreach (var kvp in _handlers)
        {
            var entities = await kvp.Value.GetAllEntityAsync();
            _logger.Log(nameof(DataRepository),$"Found {entities.Count} entities of type {kvp.Key}");
            if(entities.Count ==0) continue;

            dict.TryAdd(kvp.Key, entities);
            
        }
        
        return dict.Count >0 ? Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.SuccessResult(dict) : Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>.Failure("No entity found");
    }


    Task<IReadOnlyList<TEntity>> ILocalRepository.GetAllComponentsOfTypeAsync<TEntity>()
    {
        throw new NotImplementedException();
    }


    public Task<ResultList<TEntity>> GetAllComponentsOfTypeAsync<TEntity>()
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Result<TEntity>> SaveComponentAsync<TEntity>(TEntity component)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task<Dictionary<EntityType, IReadOnlyList<IEntity>>> GetAllAsync(CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public Task MarkDeletedAsync(EntityType type, Guid id, CancellationToken ct)
    {
        throw new NotImplementedException();
    }

    /// <inheritdoc />
    public async Task<ISyncState> GetSyncStateAsync(string partition)
    {
        _logger.Log(nameof(DataRepository), "Get Sync State");
        
        if (string.IsNullOrWhiteSpace(partition))
            partition = "global";

        var row = await _sqldbContext.SyncStates
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Partition == partition);
        
        _logger.Log(nameof(DataRepository), $"Sync State : {(row != null? row.Partition: "none")}");

        
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
        
        _logger.Log(nameof(DataRepository), $"Save Sync State ");

        
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


    /// <summary>
    /// Builds the mapping between entity types and their concrete database handlers.
    /// </summary>
    private void CreateHandlers()
    {
        _handlers = new();
        //todo for each EntityType 
        _handlers[EntityType.Equipment] = new EquipmentIdbHandler(_sqldbContext, _logger);
 
    }
    
}
