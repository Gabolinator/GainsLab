using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Interfaces.DataManagement;

/// <summary>
/// Handles serialization of components to and from file storage.
/// </summary>
public interface IFileDataService
{
    
    Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension);
    Task<ResultList<T>> LoadFromFileAsync<T>(string filePath);

    Task<Dictionary<EntityType, ResultList<IEntity>>> LoadAllComponentsAsync(); 

    public Task<Result> WriteAllComponentsAsync(Dictionary<EntityType, List<IEntity>> data, string filePath, string fileExtension);


}
