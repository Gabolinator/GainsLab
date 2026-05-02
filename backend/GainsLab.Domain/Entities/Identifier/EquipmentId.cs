namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for an equipment aggregate.
/// </summary>
public record struct EquipmentId(Guid Value)
{
    public static EquipmentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(EquipmentId id) => id.Value;
    public static explicit operator EquipmentId(Guid value) => new(value);
}