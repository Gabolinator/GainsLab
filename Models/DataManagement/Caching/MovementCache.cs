
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.Movement;

namespace GainsLab.Models.DataManagement.Caching;

public class MovementCache : BaseComponentCache<Movement>
{
    public MovementCache(IWorkoutLogger logger)
    {
        _logger = logger;
    }
    
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Movement;
}