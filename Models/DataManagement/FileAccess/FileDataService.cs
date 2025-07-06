using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement.FileAccess;

public class JsonFilesDataService  : IFileDataService
{
    private readonly IWorkoutLogger _logger;

    public JsonFilesDataService(IWorkoutLogger logger)
    {
        _logger = logger;
    }

    public Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension)
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<T>> LoadFromFileAsync<T>(string filePath)
    {
        throw new System.NotImplementedException();
    }

    public async Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> LoadAllComponentsAsync()
    {
      //todo
      return new();
    }
    
    public async Task WriteAllComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> data, string filePath, string fileExtension)
    {
        if (string.IsNullOrWhiteSpace(filePath) || !Directory.Exists(filePath))
        {
          
            _logger.LogWarning(nameof(JsonFilesDataService) ,$"Failed to Write all - Directory not found: {filePath}");
            // throw new DirectoryNotFoundException($"Directory not found: {filePath}");
            return;
        }

        if (data == null || data.Count == 0)
        {
           // throw new ArgumentException("No components provided to write.");
            _logger.LogWarning(nameof(JsonFilesDataService) ,$"Write all - No components provided to write.");
            return;
        }

        foreach (var kvp in data)
        {
            var componentType = kvp.Key;
            var components = kvp.Value;

            if (components == null || components.Count == 0)
            {
                _logger.LogWarning(nameof(JsonFilesDataService),$"No components to save for type: {componentType}");
                continue;
            }

            // Generate file name: e.g., "Equipment.json"
            string fileName = $"{componentType}{fileExtension}";
            string fullPath = Path.Combine(filePath, fileName);

            // Serialize the list to JSON array
            string jsonContent = JsonSerializer.Serialize(components, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            // Write to file asynchronously
            await File.WriteAllTextAsync(fullPath, jsonContent);
            _logger.Log(nameof(JsonFilesDataService),$"Saved {components.Count} components to {fullPath}");

            //Console.WriteLine($"[WriteAllComponentsAsync] Saved {components.Count} components to {fullPath}");
        }
    }
}