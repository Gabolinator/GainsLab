
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.Muscle;

namespace GainsLab.Models.DataManagement.Caching;

public class MusclesCache :BaseComponentCache<Muscle>
{
    
    public MusclesCache (ILogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Muscle;
}