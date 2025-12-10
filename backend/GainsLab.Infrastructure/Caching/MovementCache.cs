
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;


namespace GainsLab.Infrastructure.Caching;

public class MovementCache : BaseComponentCache<MovementId, MovementEntity>
{
    public MovementCache(ILogger logger)
    {
        _logger = logger;
    }
    
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Movement;
}