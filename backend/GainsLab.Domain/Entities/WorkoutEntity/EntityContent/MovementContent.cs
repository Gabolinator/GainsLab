using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity.EntityContent;

/// <summary>
/// Immutable data describing a movement, including workloads and supporting equipment.
/// </summary>
public sealed record MovementContent(
    string Name,
    MovementCategoryId Category,
    MuscleWorked MusclesWorked,
    EquipmentIdList EquipmentRequired,
    (string? variantOfName, MovementId? variantOfGuid) variantOf  = default //need to resolve this in some way - after all movement created
) : IEntityContent<MovementContent>
{
    public MovementContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Movement name is required.", nameof(Name));
        return this;
    }
}