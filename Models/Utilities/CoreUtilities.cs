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
        
        var eType = type.Name switch
        {
            nameof(Muscle) => eWorkoutComponents.Muscle,
            nameof(EquipmentList) => eWorkoutComponents.EquipmentList,
            nameof(MuscleGroups) => eWorkoutComponents.MuscleGroup,
            nameof(MovementCategory) => eWorkoutComponents.MovementCategory,
            nameof(Movement) => eWorkoutComponents.Movement,
            nameof(Equipment) => eWorkoutComponents.Equipment,
            nameof(WorkloadCalculationProfile) => eWorkoutComponents.WorkloadProfile,
           // nameof(Exercise) => eWorkoutComponents.Exercise,
          //  nameof(ExerciseBlock) => eWorkoutComponents.ExerciseBlock,
          //  nameof(WorkoutBlock) => eWorkoutComponents.WorkoutBlock,
          //  nameof(Workout) => eWorkoutComponents.Workout,
           // nameof(Program) => eWorkoutComponents.Program,
            _ => eWorkoutComponents.unidentified
        };
        
       // Console.WriteLine($"Component Type for {type.Name} is {eType}");
        return eType;
    }

    
}