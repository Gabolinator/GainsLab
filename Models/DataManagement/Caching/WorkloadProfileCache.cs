
using GainsLab.Models.Core;
using GainsLab.Models.WorkoutComponents;

namespace GainsLab.Models.DataManagement.Caching;

public class WorkloadProfileCache : BaseComponentCache<WorkloadCalculationProfile>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.WorkloadProfile;
   
}