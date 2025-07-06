
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.Muscle;

namespace GainsLab.Models.DataManagement.Caching;

public class MusclesGroupCache : BaseComponentCache<MusclesGroup>
{
    public MusclesGroupCache (IWorkoutLogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.MuscleGroup;
    
}

