namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for a movement aggregate.
/// </summary>
public record struct MovementId(Guid Value)
{
    public static MovementId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MovementId id) => id.Value;
    public static explicit operator MovementId(Guid value) => new(value);
}