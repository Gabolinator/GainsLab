using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for a muscle aggregate.
/// </summary>
public record struct MuscleId(Guid Value)
{
    public static MuscleId New() => new(CoreUtilities.GuidGenerator.New());
    
    public static MuscleId FromGuid(Guid id) => new(id);
    
    public static MuscleId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(MuscleId id) => id.Value;
    public static explicit operator MuscleId(Guid value) => new(value);
}