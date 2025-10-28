using System.ComponentModel.DataAnnotations;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;

namespace GainsLab.Infrastructure;

public class SyncState : ISyncState
{
    [Key]
    public string Partition { get; set; } = "global";

    // Seeding
    public bool SeedCompleted { get; set; }
    public int SeedVersion { get; set; } = 1;
    public DateTimeOffset? LastSeedAt { get; set; }
    public bool SeedInProgress { get; set; }

    // Incremental sync
    public DateTimeOffset? LastDeltaAt { get; set; }
    public string? UpstreamSnapshot { get; set; }

    // JSON blob for per-entity cursors
    public string CursorsJson { get; set; } = "{}";

    // --- Mapping helpers ---
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