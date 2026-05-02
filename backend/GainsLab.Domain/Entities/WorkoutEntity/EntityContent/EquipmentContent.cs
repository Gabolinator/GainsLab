using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity.EntityContent;

/// <summary>
/// Immutable data required to describe a single equipment item.
/// </summary>
public record EquipmentContent( string Name) : IEntityContent<EquipmentContent>
{
    
    public EquipmentContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Equipment name is required.", nameof(Name));
        return this;
    }
}