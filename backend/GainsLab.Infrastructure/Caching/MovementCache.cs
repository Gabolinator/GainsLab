using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching;
using GainsLab.Models.Logging;

namespace GainsLab.Infrastructure.Caching;

public class MovementCache : BaseComponentCache<MovementId, MovementEntity>
{
    public MovementCache(ILogger logger)
    {
        _logger = logger;
    }
    
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Movement;
}