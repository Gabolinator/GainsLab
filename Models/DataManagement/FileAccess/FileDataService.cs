using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.FileAccess;

public class JsonFilesDataService  : IFileDataService
{
    public Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension)
    {
        throw new System.NotImplementedException();
    }

    public Task<IEnumerable<T>> LoadFromFileAsync<T>(string filePath)
    {
        throw new System.NotImplementedException();
    }

    public Task<Dictionary<eWorkoutComponents, List<IWorkoutComponent>>> LoadAllComponentsAsync()
    {
        throw new System.NotImplementedException();
    }
}