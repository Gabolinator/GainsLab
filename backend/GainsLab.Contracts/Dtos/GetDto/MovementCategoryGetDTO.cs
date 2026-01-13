using System.Collections.Generic;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.GetDto;

/// <summary>
/// Response DTO describing a movement category, its descriptor metadata, and hierarchy.
/// </summary>
/// <param name="Id">Stable identifier.</param>
/// <param name="Name">Display name for the category.</param>
/// <param name="DescriptorId">Identifier of the linked descriptor.</param>
/// <param name="Descriptor">Descriptor payload returned with the category.</param>
/// <param name="ParentCategoryId">Optional parent category id.</param>
/// <param name="BaseCategories">Base categories that compose this entry.</param>
/// <param name="CreatedAtUtc">Creation timestamp tracked on the server.</param>
/// <param name="UpdatedAtUtc">Last update timestamp.</param>
/// <param name="UpdatedSeq">Monotonic sequence for sync.</param>
/// <param name="IsDeleted">Marks soft-deleted rows.</param>
/// <param name="Authority">Authority that owns the record.</param>
public record MovementCategoryGetDTO(
    Guid Id,
    string Name,
    Guid? DescriptorId,
    DescriptorGetDTO? Descriptor,
    Guid? ParentCategoryId,
    IReadOnlyList<eMovementCategories> BaseCategories,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);
