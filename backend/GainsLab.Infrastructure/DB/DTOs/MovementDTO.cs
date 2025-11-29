
using System.ComponentModel.DataAnnotations;
using GainsLab.Core.Models.Core;

namespace GainsLab.Infrastructure.DB.DTOs;

/// <summary>
/// Database representation of a movement aggregate including descriptor, category, and related entities.
/// </summary>
public class MovementDTO : BaseDto
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public Guid GUID { get; set; }

    public int DescriptorID { get; set; }

    public DescriptorDTO? Descriptor { get; set; }

    public int MovementCategoryId { get; set; }

    public MovementCategoryDTO? Category { get; set; }

    public ICollection<MovementMuscleRelationDTO> MuscleRelations { get; set; } = new List<MovementMuscleRelationDTO>();

    public ICollection<MovementEquipmentRelationDTO> EquipmentRelations { get; set; } = new List<MovementEquipmentRelationDTO>();
    
    public Guid? VariantOfMovementGuid { get; set; }
    public MovementDTO? VariantOfMovement { get; set; }
    
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
