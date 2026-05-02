using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Domain;

namespace GainsLab.Infrastructure.DB;

public static class EntityTypeComparision
{
    public static bool IsValidRecordType<T>(T entity, EntityType type)
    {
        return type switch
        {
            EntityType.Descriptor => nameof(T) == nameof(DescriptorRecord),
            EntityType.Equipment => nameof(T) == nameof(EquipmentRecord),
            EntityType.MovementCategory => nameof(T) == nameof(MovementCategoryRecord),
            EntityType.EquipmentList or
                EntityType.User or
                EntityType.Muscle or
                EntityType.MuscleGroup or
                EntityType.Movement or
                EntityType.WorkloadProfile or
                EntityType.Exercise or
                EntityType.ExerciseBlock or
                EntityType.WorkoutBlock or
                EntityType.Workout or
                EntityType.Program => throw new NotImplementedException(nameof(type)),
            _ => throw new ArgumentOutOfRangeException(nameof(type), type, null),
        };
    }
}