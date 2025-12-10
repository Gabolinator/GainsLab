using GainsLab.Application.Results;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Application.Interfaces.DataManagement;

/// <summary>
/// Coordinates loading, caching, and persistence of component data.
/// </summary>
public interface IDataManager
{

    Task InitializeAsync();

    Task<Result> LoadAndCacheDataAsync();

    Task<Result> SaveComponentAsync<TEntity>(TEntity component, bool syncUp = false) where TEntity : IEntity ;
    Task<Result<Dictionary<EntityType, IReadOnlyList<IEntity>>>> SaveComponentsAsync<TEntity>(IEnumerable<TEntity> components, bool syncUp = false) where TEntity : IEntity;
    
    Task<Result> DeleteComponentAsync<TEntity>(TEntity entity);

    Task<Result> SaveAllDataToFilesAsync();
    Task<Result> CreateLocalDataAsync();
}
