using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities.Logging;

namespace GainsLab.Core.Models.Core.Utilities;

/// <summary>
/// Provides shared singletons and helpers used across core domain code.
/// </summary>
public static class CoreUtilities
{
    
    
    
    
    // public static eWorkoutComponents GetComponentTypeFromGeneric<T>() where T : IWorkoutComponent
    // {
    //     var type = typeof(T);
    //
    //     return type switch
    //     {
    //         var t when t == typeof(Muscle) => eWorkoutComponents.Muscle,
    //         var t when t == typeof(EquipmentList) => eWorkoutComponents.EquipmentList,
    //         var t when t == typeof(MusclesGroup) => eWorkoutComponents.MuscleGroup,
    //         var t when t == typeof(MovementCategory) => eWorkoutComponents.MovementCategory,
    //         var t when t == typeof(Movement) => eWorkoutComponents.Movement,
    //         var t when t == typeof(Equipment) => eWorkoutComponents.Equipment,
    //         var t when t == typeof(WorkloadCalculationProfile) => eWorkoutComponents.WorkloadProfile,
    //         // future types (uncomment as you implement)
    //         // var t when t == typeof(Exercise) => eWorkoutComponents.Exercise,
    //         // var t when t == typeof(ExerciseBlock) => eWorkoutComponents.ExerciseBlock,
    //         // var t when t == typeof(WorkoutBlock) => eWorkoutComponents.WorkoutBlock,
    //         // var t when t == typeof(Workout) => eWorkoutComponents.Workout,
    //         // var t when t == typeof(Program) => eWorkoutComponents.Program,
    //         _ => eWorkoutComponents.unidentified
    //     };
    // }
    //

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
}
