using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Strongly-typed identifier for descriptor aggregates.
/// </summary>
public record struct DescriptorId(Guid Value)
{
    public static DescriptorId New() => new(CoreUtilities.GuidGenerator.New());
    public static DescriptorId FromGuid(Guid id) => new(id);
    
    public static DescriptorId? FromNullableGuid(Guid? id) => id != null ? new(id.Value): null;
    public override string ToString() => Value.ToString();

    public static implicit operator Guid(DescriptorId id) => id.Value;
    public static explicit operator DescriptorId(Guid value) => new(value);
}