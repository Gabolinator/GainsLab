using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure.Caching;

public class MusclesCache :BaseComponentCache<MuscleId, MuscleEntity>
{
    
    public MusclesCache (ILogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Muscle;
}