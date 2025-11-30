using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncDto;

public record MovementSyncDTO (
    Guid GUID,
    string Name,
    Guid? DescriptorGUID,
    IReadOnlyList<Guid>? PrimaryMuscles,
    IReadOnlyList<Guid>? SecondaryMuscles,
    IReadOnlyList<Guid>? Equipment,
    Guid category,
    Guid? variantOf,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;