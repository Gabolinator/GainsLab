namespace GainsLab.Infrastructure.DB;

public sealed record SeedOutcome(
    string? SnapshotVersion,                            // null for now unless your server exposes a head/etag
    IReadOnlyDictionary<string, string> Cursors,        // per-entity cursor tokens
    int EntitiesWritten,
    TimeSpan Duration
);

public sealed record DeltaOutcome(
    string? SnapshotVersion,
    IReadOnlyDictionary<string, string> Cursors,        // updated cursors after pulling
    int EntitiesUpserted,
    int EntitiesDeleted,                                // keep 0 if you don’t emit tombstones yet
    bool HadMore                                        // true if paging indicated more available
);