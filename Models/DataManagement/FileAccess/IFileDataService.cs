using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Core.Results;

namespace GainsLab.Models.DataManagement.FileAccess;

public interface IFileDataService
{
    
    Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension);
    Task<ResultList<T>> LoadFromFileAsync<T>(string filePath);

    Task<Dictionary<eWorkoutComponents, ResultList<IWorkoutComponent>>> LoadAllComponentsAsync();

    public Task<Result> WriteAllComponentsAsync(Dictionary<eWorkoutComponents, List<IWorkoutComponent>> data, string filePath, string fileExtension);


}