using System.Threading.Tasks;

namespace GainsLab.Models.App;

public interface ISystemInitializer
{
    public Task InitializeAsync();
}