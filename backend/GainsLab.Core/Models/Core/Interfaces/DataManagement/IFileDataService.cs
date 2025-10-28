using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Models.Core;

namespace GainsLab.Core.Models.Core.Interfaces.DataManagement;

/// <summary>
/// Handles serialization of components to and from file storage.
/// </summary>
public interface IFileDataService
{
    /// <summary>
    /// Serializes the supplied data set to the target file path with the given extension.
    /// </summary>
    Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension);

    /// <summary>
    /// Loads a collection of items from the specified file path.
    /// </summary>
    Task<ResultList<T>> LoadFromFileAsync<T>(string filePath);

    /// <summary>
    /// Loads all component data associated with the application.
    /// </summary>
    Task<Dictionary<EntityType, ResultList<IEntity>>> LoadAllComponentsAsync(); 

    /// <summary>
    /// Writes all components to disk using the provided destination and file format details.
    /// </summary>
    public Task<Result> WriteAllComponentsAsync(Dictionary<EntityType, List<IEntity>> data, string filePath, string fileExtension);


}
