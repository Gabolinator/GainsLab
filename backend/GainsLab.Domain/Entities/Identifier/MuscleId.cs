namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for a muscle aggregate.
/// </summary>
public record struct MuscleId(Guid Value)
{
    public static MuscleId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MuscleId id) => id.Value;
    public static explicit operator MuscleId(Guid value) => new(value);
}