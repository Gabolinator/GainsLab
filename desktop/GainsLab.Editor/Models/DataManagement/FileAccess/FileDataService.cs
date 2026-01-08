using System;
using System.Collections.Generic;

using System.Threading.Tasks;
using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Results;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;


namespace GainsLab.Models.DataManagement.FileAccess;

/// <summary>
/// File-based persistence service that will serialize components to JSON for offline storage.
/// </summary>
public class JsonFilesDataService  : IFileDataService
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonFilesDataService"/> class.
    /// </summary>
    /// <param name="logger">Logger used to emit diagnostic messages while reading or writing files.</param>
    public JsonFilesDataService(ILogger logger)
    {
        _logger = logger;
    }

    public Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension)
    {
        throw new System.NotImplementedException();
    }

    // public Task< ResultList<T>> LoadFromFileAsync<T>(string filePath)
    // {
    //     throw new System.NotImplementedException();
    // }

    public async Task<Dictionary<EntityType, IReadOnlyList<IEntity>>> LoadAllComponentsAsync()
    {
      //todo
      return new();
    }

    public Task<Result> WriteAllComponentsAsync(Dictionary<EntityType, List<IEntity>> data, string filePath, string fileExtension)
    {
        throw new NotImplementedException();
    }

    // public async Task<Dictionary<eWorkoutComponents,  ResultList<IWorkoutComponent>>> LoadAllComponentsAsync()
    // {
    //   //todo
    //   return new();
    // }
    //
    // public async Task<Result> WriteAllComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> data, string filePath, string fileExtension)
    // {
    //     if (string.IsNullOrWhiteSpace(filePath) || !Directory.Exists(filePath))
    //     {
    //       
    //         _logger.LogWarning(nameof(JsonFilesDataService) ,$"Failed to Write all - Directory not found: {filePath}");
    //         // throw new DirectoryNotFoundException($"Directory not found: {filePath}");
    //         return Result.Failure("Not a valid Directory");
    //     }
    //
    //     if (data == null || data.Count == 0)
    //     {
    //        // throw new ArgumentException("No components provided to write.");
    //         _logger.LogWarning(nameof(JsonFilesDataService) ,$"Write all - No components provided to write.");
    //         return Result.Failure("No components provided to write");
    //     }
    //
    //     int wroteComponent = 0;
    //     foreach (var kvp in data)
    //     {
    //         var componentType = kvp.Key;
    //         var components = kvp.Value;
    //
    //         if (components == null || components.Count == 0)
    //         {
    //             _logger.LogWarning(nameof(JsonFilesDataService),$"No components to save for type: {componentType}");
    //             continue;
    //         }
    //
    //         // Generate file name: e.g., "Equipment.json"
    //         string fileName = $"{componentType}{fileExtension}";
    //         string fullPath = Path.Combine(filePath, fileName);
    //
    //         // Serialize the list to JSON array
    //         string jsonContent = JsonSerializer.Serialize(components, new JsonSerializerOptions
    //         {
    //             WriteIndented = true
    //         });
    //
    //         // Write to file asynchronously
    //         await File.WriteAllTextAsync(fullPath, jsonContent);
    //         _logger.Log(nameof(JsonFilesDataService),$"Saved {components.Count} components to {fullPath}");
    //         wroteComponent++;
    //         //Console.WriteLine($"[WriteAllComponentsAsync] Saved {components.Count} components to {fullPath}");
    //     }
    //
    //     return wroteComponent != 0 ? Result.SuccessResult() : Result.Failure("Wrote 0 componenets to file");
    // }
}
