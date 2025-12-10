using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Interfaces.Sync;


namespace GainsLab.Infrastructure;

/// <summary>
/// Persists the current synchronization status for a logical partition so that incremental syncs can resume safely.
/// </summary>
public class SyncState : ISyncState
{
    /// <summary>
    /// The logical partition key for which this state is tracked. Defaults to the global partition.
    /// </summary>
    [Key]
    public string Partition { get; set; } = "global";

    // Seeding
    /// <summary>
    /// Gets or sets a value indicating whether the initial seed run finished successfully.
    /// </summary>
    public bool SeedCompleted { get; set; }

    /// <summary>
    /// The schema or data version applied during the last seed operation.
    /// </summary>
    public int SeedVersion { get; set; } = 1;

    /// <summary>
    /// Timestamp for the last completed seed, or <c>null</c> if seeding never ran.
    /// </summary>
    public DateTimeOffset? LastSeedAt { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether a seed operation is currently in-flight.
    /// </summary>
    public bool SeedInProgress { get; set; }

    // Incremental sync
    /// <summary>
    /// Timestamp for the last incremental delta sync that finished.
    /// </summary>
    public DateTimeOffset? LastDeltaAt { get; set; }

    /// <summary>
    /// Remote snapshot identifier returned by the upstream service for the current delta.
    /// </summary>
    public string? UpstreamSnapshot { get; set; }

    /// <summary>
    /// Serialized JSON payload that stores entity-specific cursor positions.
    /// </summary>
    public string CursorsJson { get; set; } = "{}";

    // Mapping helpers
    /// <summary>
    /// Creates a deep copy of the supplied <paramref name="src"/> state.
    /// </summary>
    public static SyncState From(SyncState src) => new()
    {
        Partition        = src.Partition,
        SeedCompleted    = src.SeedCompleted,
        SeedVersion      = src.SeedVersion,
        LastSeedAt       = src.LastSeedAt,
        SeedInProgress   = src.SeedInProgress,
        LastDeltaAt      = src.LastDeltaAt,
        UpstreamSnapshot = src.UpstreamSnapshot,
        CursorsJson      = src.CursorsJson ?? "{}"
    };

    /// <summary>
    /// Copies the mutable synchronization fields from <paramref name="src"/> into <paramref name="dest"/>.
    /// </summary>
    public static void Copy(SyncState src, SyncState dest)
    {
        dest.SeedCompleted    = src.SeedCompleted;
        dest.SeedVersion      = src.SeedVersion;
        dest.LastSeedAt       = src.LastSeedAt;
        dest.SeedInProgress   = src.SeedInProgress;
        dest.LastDeltaAt      = src.LastDeltaAt;
        dest.UpstreamSnapshot = src.UpstreamSnapshot;
        dest.CursorsJson      = string.IsNullOrWhiteSpace(src.CursorsJson) ? "{}" : src.CursorsJson!;
    }
}
