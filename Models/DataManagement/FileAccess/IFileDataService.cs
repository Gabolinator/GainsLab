using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.DataManagement.FileAccess;

public interface IFileDataService
{
    
    Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension);
    Task<IEnumerable<T>> LoadFromFileAsync<T>(string filePath);

    Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> LoadAllComponentsAsync();

    public Task WriteAllComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> data, string filePath, string fileExtension);


}