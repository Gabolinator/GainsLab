using System.Collections.Generic;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.SyncDto;

/// <summary>
/// Synchronization payload describing a movement category, descriptor link, and hierarchy metadata.
/// </summary>
/// <param name="GUID">Stable identifier for the movement category.</param>
/// <param name="Name">Display name.</param>
/// <param name="DescriptorGUID">Descriptor reference associated with the category.</param>
/// <param name="ParentCategoryGUID">Optional parent category identifier.</param>
/// <param name="BaseCategories">Base categories composing this entry.</param>
/// <param name="UpdatedAtUtc">Timestamp of last modification.</param>
/// <param name="UpdatedSeq">Monotonic sequence for conflict resolution.</param>
/// <param name="IsDeleted">Marks tombstones.</param>
/// <param name="Authority">Indicates who owns the record.</param>
public record MovementCategorySyncDTO(
    Guid GUID,
    string Name,
    Guid? DescriptorGUID,
    Guid? ParentCategoryGUID,
    IReadOnlyList<eMovementCategories> BaseCategories,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;
