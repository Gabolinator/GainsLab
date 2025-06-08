using System.Collections.Generic;
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;
using GainsLab.Models.DataManagement.Caching.Interface;

namespace GainsLab.Models.DataManagement.Caching;

public class MusclesCache :BaseComponentCache<Muscle>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Muscle;
}