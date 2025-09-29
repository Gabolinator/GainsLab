namespace GainsLab.Core.Models.Core.Entities.Identifier;

public record struct MovementId(Guid Value)
{
    public static MovementId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MovementId id) => id.Value;
    public static explicit operator MovementId(Guid value) => new(value);
}

public record struct MovementCategoryId(Guid Value)
{
    public static MovementCategoryId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MovementCategoryId id) => id.Value;
    public static explicit operator MovementCategoryId(Guid value) => new(value);
}


public record struct MuscleId(Guid Value)
{
    public static MuscleId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MuscleId id) => id.Value;
    public static explicit operator MuscleId(Guid value) => new(value);
}

public record struct EquipmentId(Guid Value)
{
    public static EquipmentId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(EquipmentId id) => id.Value;
    public static explicit operator EquipmentId(Guid value) => new(value);
}



public record struct DescriptorId(Guid Value)
{
    public static DescriptorId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DescriptorId id) => id.Value;
    public static explicit operator DescriptorId(Guid value) => new(value);
}

public record struct UserId(Guid Value)
{
    public static UserId New() => new(Guid.NewGuid());
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(UserId id) => id.Value;
    public static explicit operator UserId(Guid value) => new(value);
}

