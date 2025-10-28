
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.Caching;

public class MusclesCache :BaseComponentCache<MuscleId, MuscleEntity>
{
    
    public MusclesCache (ILogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Muscle;
}