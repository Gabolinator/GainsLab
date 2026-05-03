using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for a movement category aggregate.
/// </summary>
public record struct MovementCategoryId(Guid Value)
{
    public static MovementCategoryId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static MovementCategoryId FromGuid(Guid id) => new(id);
    
    public static MovementCategoryId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MovementCategoryId id) => id.Value;
    public static explicit operator MovementCategoryId(Guid value) => new(value);
}