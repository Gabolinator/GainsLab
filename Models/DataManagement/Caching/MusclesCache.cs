
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;

using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement.Caching;

public class MusclesCache :BaseComponentCache<Muscle>
{
    
    public MusclesCache (IWorkoutLogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Muscle;
}