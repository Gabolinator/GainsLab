using System.Threading.Tasks;

namespace GainsLab.Models.DataManagement;

public interface IDataProvider
{
    Task InitializeAsync();
}