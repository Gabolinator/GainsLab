using System.Collections.Generic;
using System.Threading.Tasks;

namespace GainsLab.Models.DataManagement.FileAccess;

public interface IFileDataService
{
    
    Task SaveToFileAsync<T>(IEnumerable<T> data, string filePath, string fileExtension);
    Task<IEnumerable<T>> LoadFromFileAsync<T>(string filePath);
}