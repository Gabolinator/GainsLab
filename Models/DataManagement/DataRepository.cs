using System.Threading.Tasks;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement;

public class DataRepository : IDataProvider
{
    
    private readonly IWorkoutLogger _workoutLogger;

    public DataRepository(IWorkoutLogger workoutLogger)
    {
        _workoutLogger = workoutLogger;
    }
    
    public async Task InitializeAsync()
    {
        _workoutLogger.Log(nameof(DataRepository), "Loading Data started");
        await Task.Delay(500);
        _workoutLogger.Log(nameof(DataRepository), "Loading Data finished");
    }
}