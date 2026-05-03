using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for a movement aggregate.
/// </summary>
public record struct MovementId(Guid Value)
{
    public static MovementId New() => new(CoreUtilities.GuidGenerator.New());
    public override string ToString() => Value.ToString();
    
    public static MovementId FromGuid(Guid id) => new(id);
    
    public static MovementId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;

    public static implicit operator Guid(MovementId id) => id.Value;
    public static explicit operator MovementId(Guid value) => new(value);
}