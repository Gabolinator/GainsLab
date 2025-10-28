using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Core.Models.Core.Interfaces.DataManagement;

public interface ILocalRepository
{
    Task<Result> InitializeAsync();
    Task<Result<TEntity>> GetComponentAsync<TId, TEntity>(TId id);
    Task<ResultList<TEntity>> GetComponentsAsync<TId, TEntity>(List<TId> ids);

    Task<Result> DeleteComponentAsync<TId, TEntity>(TId id);
    Task<ResultList<TEntity>> SaveComponentsAsync<TId, TEntity>(EntityType componentType, List<TEntity> list);

    Task<Result<Dictionary<EntityType, ResultList<TEntity>>>> BatchSaveComponentsAsync<TEntity>(
        Dictionary<EntityType, ResultList<TEntity>> fileData);

    Task<Result<Dictionary<EntityType, ResultList<TEntity>>>> GetAllComponentsAsync<TEntity>();
    Task<ResultList<TEntity>> GetAllComponentsOfTypeAsync<TEntity>();

    Task<Result<TEntity>> SaveComponentAsync<TEntity>(TEntity component);

    
    Task<Dictionary<EntityType, ResultList<IEntity>>> GetAllAsync(CancellationToken ct);
    
    Task MarkDeletedAsync(EntityType type, Guid id, CancellationToken ct);
}

  
   