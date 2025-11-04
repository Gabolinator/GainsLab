using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Core.Models.Core.Interfaces.DataManagement;

/// <summary>
/// Coordinates loading, caching, and persistence of component data.
/// </summary>
public interface IDataManager
{

    Task InitializeAsync();

    Task<Result> LoadAndCacheDataAsync();
    
    // Task<Result<TEntity>> TryGetEntityAsync<TId, TEntity>(TId id);
    //
    // Task<Result<IReadOnlyList<TEntity>>> TryGetComponentsAsync<TId, TEntity>(IEnumerable<TId> ids);

   
    Task<Result> SaveComponentAsync<TEntity>(TEntity component, bool syncUp = false) where TEntity : IEntity ;
    Task<Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>> SaveComponentsAsync<TEntity>(IEnumerable<TEntity> components, bool syncUp = false) where TEntity : IEntity;

   // Task<Result<IReadOnlyList<TEntity>>> TryResolveComponentsAsync<TId,TEntity>(List<TId> toResolve);
   // Task<Result<TEntity>> TryResolveComponentAsync<TId,TEntity>(TId unresolved);
    Task<Result> DeleteComponentAsync<TEntity>(TEntity entity);

    Task<Result> SaveAllDataToFilesAsync();
}
