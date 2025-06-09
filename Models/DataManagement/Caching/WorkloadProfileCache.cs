
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents;

namespace GainsLab.Models.DataManagement.Caching;

public class WorkloadProfileCache : BaseComponentCache<WorkloadCalculationProfile>
{
    public WorkloadProfileCache (IWorkoutLogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.WorkloadProfile;
   
}