
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.MovementCategory;

namespace GainsLab.Models.DataManagement.Caching;

public class MovementCategoryCache : BaseComponentCache<MovementCategory>
{
    
    public MovementCategoryCache (IWorkoutLogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.MovementCategory;
   
}