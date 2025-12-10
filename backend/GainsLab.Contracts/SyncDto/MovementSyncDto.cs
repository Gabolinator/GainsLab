using GainsLab.Contracts.Interface;
using GainsLab.Domain;

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