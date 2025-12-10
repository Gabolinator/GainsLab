namespace GainsLab.Domain;


public static class TypeConverter
{
    public static EntityType GetEntityTye(this DTOEntityType dto)
    {
        return dto switch
        {
            DTOEntityType.UserDTO =>  EntityType.User,
            DTOEntityType.DescriptorDTO =>  EntityType.Descriptor,
            DTOEntityType.EquipmentDTO => EntityType.Equipment,
            DTOEntityType.MuscleDTO => EntityType.Muscle,
            _ => EntityType.unidentified
        };
    }
}

/// <summary>
/// Identifies the high-level domain entity represented within the system.
/// </summary>
public enum EntityType
{
    User, 
    Descriptor,
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

public enum DTOEntityType
{
    UserDTO, 
    DescriptorDTO,
    EquipmentDTO,
    MuscleDTO,
    unidentified
}

public enum SyncDTOType
{
    UserSyncDTO, 
    DescriptorSyncDTO,
    EquipmentSyncDTO,
    MuscleSyncDTO,
    unidentified
}


public enum MuscleRole
{
    Primary,
    Secondary
}

/// <summary>
/// Lists the workout-oriented component categories that can appear in programs.
/// </summary>
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

/// <summary>
/// Captures the kinds of execution metadata supported for workout content.
/// </summary>
public enum eExecutionDetails
{
    TimeDetails, //related to time - prepare time , time under tension , time cap , etc 
    WeightDetails, //weight , or percentage body weight or 
    DistanceDetails, //
    TempoDetail,
    unidentified//
}
    
/// <summary>
/// Enumerates base movement categories used to group exercises.
/// </summary>
public enum eMovementCategories
{
    BodyWeight,
    Weightlifting,
    Cardio,
    Flexibility,
    Hybrid,
    undefined
}
    
/// <summary>
/// Represents sections of the body targeted by a movement or muscle.
/// </summary>
public enum eBodySection
{
    UpperBody,
    MidSection,
    LowerBody,
    undefined
}
