namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for descriptor aggregates.
/// </summary>
public record struct DescriptorId(Guid Value)
{
    public static DescriptorId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DescriptorId id) => id.Value;
    public static explicit operator DescriptorId(Guid value) => new(value);
}