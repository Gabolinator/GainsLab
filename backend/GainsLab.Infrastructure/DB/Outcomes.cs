namespace GainsLab.Infrastructure.DB;

/// <summary>
/// Represents the result of running a full seed synchronization.
/// </summary>
/// <param name="SnapshotVersion">Optional upstream snapshot version returned by the server.</param>
/// <param name="Cursors">Cursor tokens keyed by entity name.</param>
/// <param name="EntitiesWritten">Number of entities persisted locally.</param>
/// <param name="Duration">Total runtime of the seed pass.</param>
public sealed record SeedOutcome(
    string? SnapshotVersion,                            // null for now unless your server exposes a head/etag
    IReadOnlyDictionary<string, string> Cursors,        // per-entity cursor tokens
    int EntitiesWritten,
    TimeSpan Duration
);

/// <summary>
/// Represents the result of running an incremental delta synchronization.
/// </summary>
/// <param name="SnapshotVersion">Optional upstream snapshot version returned by the server.</param>
/// <param name="Cursors">Updated cursor tokens keyed by entity name.</param>
/// <param name="EntitiesUpserted">Number of entities inserted or updated.</param>
/// <param name="EntitiesDeleted">Number of entities marked deleted during the run.</param>
/// <param name="HadMore">Indicates whether the server signaled additional pages.</param>
public sealed record DeltaOutcome(
    string? SnapshotVersion,
    IReadOnlyDictionary<string, string> Cursors,        // updated cursors after pulling
    int EntitiesUpserted,
    int EntitiesDeleted,                                // keep 0 if you don’t emit tombstones yet
    bool HadMore                                        // true if paging indicated more available
);
