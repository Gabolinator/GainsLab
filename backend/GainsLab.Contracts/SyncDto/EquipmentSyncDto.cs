using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncDto;

/// <summary>
/// Synchronization DTO for equipment entities transported through the sync pipeline.
/// </summary>
/// <param name="GUID">The stable identifier for the equipment.</param>
/// <param name="Name">The display name associated with the equipment.</param>
/// <param name="DescriptorGUID">Optional descriptor identifier referenced by the equipment.</param>
/// <param name="UpdatedAtUtc">Server timestamp representing the last modification moment.</param>
/// <param name="UpdatedSeq">Sequence number used to break ties when timestamps are equal.</param>
/// <param name="IsDeleted">Flag indicating whether the equipment represents a tombstone.</param>
public record EquipmentSyncDto(
    Guid GUID,
    string Name,
    Guid? DescriptorGUID,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false) : ISyncDto;
