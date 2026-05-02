namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for a movement category aggregate.
/// </summary>
public record struct MovementCategoryId(Guid Value)
{
    public static MovementCategoryId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MovementCategoryId id) => id.Value;
    public static explicit operator MovementCategoryId(Guid value) => new(value);
}