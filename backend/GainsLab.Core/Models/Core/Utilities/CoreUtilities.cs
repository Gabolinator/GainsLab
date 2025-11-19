using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities.Logging;

namespace GainsLab.Core.Models.Core.Utilities;

/// <summary>
/// Provides shared singletons and helpers used across core domain code.
/// </summary>
public static class CoreUtilities
{
    
    public static GainsLabLogger? _logger;
    
    /// <summary>
    /// Lazily initialized logger used when no explicit logger is provided.
    /// </summary>
    public static GainsLabLogger Logger => _logger ?? new GainsLabLogger();

    private static IClock? _clock;
    /// <summary>
    /// Lazily initialized clock abstraction for consistent time access.
    /// </summary>
    public static IClock Clock => _clock ?? new Clock();
    
    
    private static readonly EntityType[] SyncOrder =
    {
        EntityType.User,
        EntityType.Descriptor,
        EntityType.MovementCategory,
        EntityType.MuscleGroup,
        EntityType.Muscle,
        EntityType.Equipment,
        EntityType.EquipmentList,
        EntityType.WorkloadProfile,
        EntityType.Movement,       // depends on Category, Muscles, Equipment
        EntityType.Exercise,       // depends on Movement, WorkloadProfile
        EntityType.ExerciseBlock,  // depends on Exercise
        EntityType.WorkoutBlock,   // depends on ExerciseBlock
        EntityType.Workout,        // depends on WorkoutBlock
        EntityType.Program,        // depends on Workout
    };
    
    public static readonly IReadOnlyDictionary<EntityType, int> Rank =
        SyncOrder.Select((t, i) => (t, i)).ToDictionary(x => x.t, x => x.i);

    public static int RankOf(this EntityType t) =>
        Rank.GetValueOrDefault(t, int.MaxValue); // unknowns (e.g., unidentified) go last
    
}
