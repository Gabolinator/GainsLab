
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;

namespace GainsLab.Models.DataManagement.Caching;

public class MusclesGroupCache : BaseComponentCache<MusclesGroup>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.MuscleGroup;
    
}

