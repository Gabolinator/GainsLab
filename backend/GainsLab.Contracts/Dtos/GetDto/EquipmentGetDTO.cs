using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.GetDto;

/// <summary>
/// Response DTO describing a single equipment entity and its descriptor metadata.
/// </summary>
/// <param name="Id">Stable equipment identifier.</param>
/// <param name="Name">Display name for the equipment.</param>
/// <param name="DescriptorId">Identifier of the linked descriptor, if any.</param>
/// <param name="Descriptor">Projected descriptor payload returned alongside the equipment.</param>
/// <param name="CreatedAtUtc">Creation timestamp tracked on the server.</param>
/// <param name="UpdatedAtUtc">Last update timestamp tracked on the server.</param>
/// <param name="UpdatedSeq">Monotonic sequence for conflict resolution.</param>
/// <param name="IsDeleted">Whether this record represents a tombstone.</param>
/// <param name="Authority">Tier that owns the record.</param>
public record EquipmentGetDTO(
    Guid Id,
    string Name,
    Guid? DescriptorId,
    DescriptorGetDTO? Descriptor,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);


