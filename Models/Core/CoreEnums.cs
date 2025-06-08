namespace GainsLab.Models.Core;


    public enum eWorkoutComponents
    {
        Equipment,
        EquipmentList,
        MovementCategory,
        Muscle,
        MuscleGroup,
        Movement,
        WorkloadProfile,
        Exercise,
        ExerciseBlock,
        WorkoutBlock,
        Workout,
        Program,
        unidentified
    }

    public enum eExecutionDetails
    {
        TimeDetails, //related to time - prepare time , time under tension , time cap , etc 
        WeightDetails, //weight , or percentage body weight or 
        DistanceDetails, //
        TempoDetail,
        unidentified//
    }
    
    public enum eMovementCategories
    {
       BodyWeight,
       Weightlifting,
       Cardio,
       Flexibility,
       Hybrid,
       undefined
    }
    
    public enum eBodySection
    {
        UpperBody,
        MidSection,
        LowerBody,
        undefined

    }