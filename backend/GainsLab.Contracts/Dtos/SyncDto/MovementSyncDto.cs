using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.SyncDto;

public record MovementSyncDTO (
    MovementCategoryId GUID,
    string Name,
    DescriptorId? DescriptorGUID,
    IReadOnlyList<Guid>? PrimaryMuscles,
    IReadOnlyList<Guid>? SecondaryMuscles,
    IReadOnlyList<Guid>? Equipment,
    Guid category,
    Guid? variantOf,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;