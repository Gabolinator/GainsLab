using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement.FileAccess;

public class JsonFilesDataService  : IFileDataService
{
    private readonly ILogger _logger;

    public JsonFilesDataService(ILogger logger)
    {
        _logger = logger;
    }

    public Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension)
    {
        throw new System.NotImplementedException();
    }

    public Task< ResultList<T>> LoadFromFileAsync<T>(string filePath)
    {
        throw new System.NotImplementedException();
    }

    public Task<Dictionary<eWorkoutComponents, ResultList<IEntity>>> LoadAllComponentsAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Result> WriteAllComponentsAsync(Dictionary<eWorkoutComponents, List<IEntity>> data, string filePath, string fileExtension)
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