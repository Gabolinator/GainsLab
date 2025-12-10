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

