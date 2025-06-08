
using GainsLab.Models.Core;
using GainsLab.Models.WorkoutComponents.MovementCategory;

namespace GainsLab.Models.DataManagement.Caching;

public class MovementCategoryCache : BaseComponentCache<MovementCategory>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.MovementCategory;
   
}