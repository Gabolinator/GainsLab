using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement.DB;

public interface IDataProvider
{
    Task<Result> InitializeAsync();
    Task<Result <TEntity>> GetComponentAsync <TId,TEntity>(TId id)  ;
    Task<ResultList <TEntity>> GetComponentsAsync <TId,TEntity>(List<TId> ids)  ;
    
    Task<Result> DeleteComponentAsync <TId,TEntity>(TId id)  ;
    Task<ResultList <TEntity>> SaveComponentsAsync <TId,TEntity>(eWorkoutComponents componentType, List <TEntity> list)  ;
    Task<Result<Dictionary<eWorkoutComponents, ResultList< TEntity>>>> BatchSaveComponentsAsync<TEntity>(Dictionary<eWorkoutComponents, ResultList< TEntity>> fileData);
    Task<Result<Dictionary<eWorkoutComponents,ResultList< TEntity>>>> GetAllComponentsAsync<TEntity>();
    Task<ResultList<TEntity>> GetAllComponentsOfTypeAsync<TEntity>();
    
    Task<Result <TEntity>> SaveComponentAsync <TEntity>(TEntity component)  ;
}