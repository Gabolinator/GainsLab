using System;
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;
using GainsLab.Models.DataManagement;
using GainsLab.Models.WorkoutComponents;
using GainsLab.Models.WorkoutComponents.Movement;
using GainsLab.Models.WorkoutComponents.MovementCategory;


namespace GainsLab.Models.Utilities;

public static class CoreUtilities
{
    public static eWorkoutComponents GetComponentTypeFromGeneric<T>() where T : IWorkoutComponent
    {
        var type = typeof(T);

        return type switch
        {
            var t when t == typeof(Muscle) => eWorkoutComponents.Muscle,
            var t when t == typeof(EquipmentList) => eWorkoutComponents.EquipmentList,
            var t when t == typeof(MusclesGroup) => eWorkoutComponents.MuscleGroup,
            var t when t == typeof(MovementCategory) => eWorkoutComponents.MovementCategory,
            var t when t == typeof(Movement) => eWorkoutComponents.Movement,
            var t when t == typeof(Equipment) => eWorkoutComponents.Equipment,
            var t when t == typeof(WorkloadCalculationProfile) => eWorkoutComponents.WorkloadProfile,
            // future types (uncomment as you implement)
            // var t when t == typeof(Exercise) => eWorkoutComponents.Exercise,
            // var t when t == typeof(ExerciseBlock) => eWorkoutComponents.ExerciseBlock,
            // var t when t == typeof(WorkoutBlock) => eWorkoutComponents.WorkoutBlock,
            // var t when t == typeof(Workout) => eWorkoutComponents.Workout,
            // var t when t == typeof(Program) => eWorkoutComponents.Program,
            _ => eWorkoutComponents.unidentified
        };
    }

    
}