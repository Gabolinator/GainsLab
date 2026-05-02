namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for user aggregates.
/// </summary>
public record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;
    public static explicit operator UserId(Guid value) => new(value);
}