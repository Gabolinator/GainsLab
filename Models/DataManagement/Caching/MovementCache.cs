using System.Collections.Generic;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching.Interface;
using GainsLab.Models.WorkoutComponents.Movement;

namespace GainsLab.Models.DataManagement.Caching;

public class MovementCache : BaseComponentCache<Movement>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Movement;
}