using System.Text.Json;

namespace GainsLab.Contracts;

public sealed record SyncPushEnvelope(
    DateTimeOffset ClientTime,
    IReadOnlyList<JsonElement> Items // raw; each service will deserialize to its TSyncDto
);

public enum PushItemStatus { Upserted, Deleted, SkippedDuplicate, Conflict, NotFound, Failed }

public sealed record PushItemResult(Guid Id, PushItemStatus Status, string? Message = null);

public sealed record PushResult(
    DateTimeOffset ServerTime,
    int Accepted,
    int Failed,
    IReadOnlyList<PushItemResult> Items
);