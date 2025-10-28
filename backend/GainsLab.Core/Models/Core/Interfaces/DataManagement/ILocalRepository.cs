using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Core.Models.Core.Interfaces.DataManagement;

/// <summary>
/// Abstraction over the local persistence layer used by the desktop application.
/// </summary>
public interface ILocalRepository
{
    /// <summary>
    /// Performs startup initialization such as migrations or connection warm-up.
    /// </summary>
    Task<Result> InitializeAsync();

    /// <summary>
    /// Retrieves a single component by its identifier.
    /// </summary>
    Task<Result<TEntity>> GetComponentAsync<TId, TEntity>(TId id);

    /// <summary>
    /// Retrieves a set of components matching the supplied identifiers.
    /// </summary>
    Task<ResultList<TEntity>> GetComponentsAsync<TId, TEntity>(List<TId> ids);

    /// <summary>
    /// Deletes a component from the local store.
    /// </summary>
    Task<Result> DeleteComponentAsync<TId, TEntity>(TId id);

    /// <summary>
    /// Persists a batch of components for a single entity type.
    /// </summary>
    Task<ResultList<TEntity>> SaveComponentsAsync<TId, TEntity>(EntityType componentType, List<TEntity> list);

    /// <summary>
    /// Saves the provided file payloads to the local store in bulk.
    /// </summary>
    Task<Result<Dictionary<EntityType, ResultList<TEntity>>>> BatchSaveComponentsAsync<TEntity>(
        Dictionary<EntityType, ResultList<TEntity>> fileData);

    /// <summary>
    /// Retrieves all components grouped by entity type.
    /// </summary>
    Task<Result<Dictionary<EntityType, ResultList<TEntity>>>> GetAllComponentsAsync<TEntity>();

    /// <summary>
    /// Retrieves all components of a specific entity type.
    /// </summary>
    Task<ResultList<TEntity>> GetAllComponentsOfTypeAsync<TEntity>();

    /// <summary>
    /// Persists a single component and returns the saved instance.
    /// </summary>
    Task<Result<TEntity>> SaveComponentAsync<TEntity>(TEntity component);

    /// <summary>
    /// Retrieves all components grouped by type, honoring the provided cancellation token.
    /// </summary>
    Task<Dictionary<EntityType, ResultList<IEntity>>> GetAllAsync(CancellationToken ct);

    /// <summary>
    /// Marks a component as deleted without removing its record.
    /// </summary>
    Task MarkDeletedAsync(EntityType type, Guid id, CancellationToken ct);

    /// <summary>
    /// Retrieves the synchronization state for the supplied partition key.
    /// </summary>
    Task<ISyncState> GetSyncStateAsync(string s);

    /// <summary>
    /// Persists the supplied synchronization state.
    /// </summary>
    Task SaveSyncStateAsync(ISyncState state);
}
