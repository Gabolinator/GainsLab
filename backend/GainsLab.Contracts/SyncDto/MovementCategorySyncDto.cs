using System.Collections.Generic;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncDto;

public record MovementCategorySyncDto(
    Guid GUID,
    string Name,
    Guid? DescriptorGUID,
    Guid? ParentCategoryGUID,
    IReadOnlyList<eMovementCategories> BaseCategories,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;
