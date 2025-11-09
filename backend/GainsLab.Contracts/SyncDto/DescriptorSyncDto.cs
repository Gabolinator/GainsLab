using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncDto;

/// <summary>
/// Synchronization DTO for descriptor entities exposed by the remote API.
/// </summary>
/// <param name="GUID">The stable identifier of the descriptor.</param>
/// <param name="DescriptionContent">The serialized descriptor payload.</param>
/// <param name="UpdatedAtUtc">Server timestamp for the last mutation.</param>
/// <param name="UpdatedSeq">Sequence number used to break ties when timestamps are equal.</param>
/// <param name="IsDeleted">Flag indicating whether the descriptor represents a tombstone.</param>
/// <param name="Authority">Indicates which tier owns the record.</param>
public record DescriptorSyncDTO(
    Guid GUID,
    string DescriptionContent,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;
