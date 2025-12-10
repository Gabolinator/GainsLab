using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Application.DTOs;

/// <summary>
/// Database representation of a movement aggregate including descriptor, category, and related entities.
/// </summary>
public class MovementRecord : BaseRecord
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid GUID { get; set; }

    public int DescriptorID { get; set; }

    public DescriptorRecord? Descriptor { get; set; }

    public int MovementCategoryId { get; set; }

    public MovementCategoryRecord? Category { get; set; }

    public ICollection<MovementMuscleRelationRecord> MuscleRelations { get; set; } = new List<MovementMuscleRelationRecord>();

    public ICollection<MovementEquipmentRelationRecord> EquipmentRelations { get; set; } = new List<MovementEquipmentRelationRecord>();
    
    public Guid? VariantOfMovementGuid { get; set; }
    public MovementRecord? VariantOfMovement { get; set; }
    
    public override int Iid => Id;
    public override Guid Iguid => GUID;
    public override string? GetContent() => Name;

    public override EntityType Type => EntityType.Movement;

    /// <summary>
    /// Convenience accessors used by movement mappers.
    /// </summary>
    public IEnumerable<Guid> PrimaryMuscleGUIDs =>
        MuscleRelations
            .Where(relation => relation.MuscleRole == MuscleRole.Primary && relation.Muscle != null)
            .Select(relation => relation.Muscle.GUID)
            .Distinct();

    public IEnumerable<Guid> SecondaryMuscleGUIDs =>
        MuscleRelations
            .Where(relation => relation.MuscleRole == MuscleRole.Secondary && relation.Muscle != null)
            .Select(relation => relation.Muscle.GUID)
            .Distinct();

    public IEnumerable<Guid> EquipmentGUIDs =>
        EquipmentRelations
            .Where(relation => relation.Equipment != null)
            .Select(relation => relation.Equipment.GUID)
            .Distinct();
}
