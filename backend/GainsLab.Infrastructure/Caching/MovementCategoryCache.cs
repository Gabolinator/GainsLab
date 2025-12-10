
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure.Caching;

public class MovementCategoryCache : BaseComponentCache<MovementCategoryId, MovementCategoryEntity>
{
    
    public MovementCategoryCache (ILogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.MovementCategory;
   
}