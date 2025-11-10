using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncDto;

/// <summary>
/// Synchronization payload describing a muscle, its descriptor link, and antagonists.
/// </summary>
/// <param name="GUID">Stable identifier for the muscle.</param>
/// <param name="Name">Display name.</param>
/// <param name="DescriptorGUID">Descriptor reference associated with the muscle.</param>
/// <param name="BodySection">Body section classification.</param>
/// <param name="AntagonistGuids">Optional list of antagonist muscle GUIDs.</param>
/// <param name="UpdatedAtUtc">Timestamp of last modification.</param>
/// <param name="UpdatedSeq">Monotonic sequence for conflict resolution.</param>
/// <param name="IsDeleted">Marks tombstones.</param>
/// <param name="Authority">Indicates who owns the record.</param>
public record MuscleSyncDTO(
    Guid GUID,
    string Name,
    Guid? DescriptorGUID,
    eBodySection BodySection,
    IReadOnlyList<Guid>? AntagonistGuids,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;
