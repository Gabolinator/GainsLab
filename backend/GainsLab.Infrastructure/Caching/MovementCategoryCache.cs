using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching;

namespace GainsLab.Infrastructure.Caching;

public class MovementCategoryCache : BaseComponentCache<MovementCategoryId, MovementCategoryEntity>
{
    
    public MovementCategoryCache (ILogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.MovementCategory;
   
}