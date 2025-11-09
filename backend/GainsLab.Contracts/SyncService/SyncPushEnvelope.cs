using System.Text.Json;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Represents a client-submitted batch of sync payloads to be processed by the server.
/// </summary>
/// <param name="ClientTime">Timestamp recorded by the client for auditing purposes.</param>
/// <param name="Items">Raw JSON elements that are deserialized into the service-specific DTO type.</param>
public sealed record SyncPushEnvelope(
    DateTimeOffset ClientTime,
    IReadOnlyList<JsonElement> Items // raw; each service will deserialize to its TSyncDto
)
{
    public string GetString()
    {
        return string.Join(',', Items);
    }
};

/// <summary>
/// Enumerates the possible outcomes for an individual push item.
/// </summary>
public enum PushItemStatus
{
    /// <summary>Item was inserted or updated successfully.</summary>
    Upserted,
    /// <summary>Item was removed.</summary>
    Deleted,
    /// <summary>Item was skipped because the server already has a newer version.</summary>
    SkippedDuplicate,
    /// <summary>Item conflicted with existing data.</summary>
    Conflict,
    /// <summary>Item could not be located for the requested operation.</summary>
    NotFound,
    /// <summary>Item failed to process due to an unexpected error.</summary>
    Failed
}

/// <summary>
/// Represents the server's response for a single pushed item.
/// </summary>
/// <param name="Id">Identifier of the item that was processed.</param>
/// <param name="Status">Outcome returned by the server.</param>
/// <param name="Message">Optional message describing the status.</param>
public sealed record PushItemResult(Guid Id, PushItemStatus Status, string? Message = null);

/// <summary>
/// Aggregated response describing the overall outcome of a push batch.
/// </summary>
/// <param name="ServerTime">Timestamp recorded when the server processed the batch.</param>
/// <param name="Accepted">Number of items accepted by the server.</param>
/// <param name="Failed">Number of items that failed processing.</param>
/// <param name="Items">Detailed results for each item in the batch.</param>
public sealed record PushResult(
    DateTimeOffset ServerTime,
    int Accepted,
    int Failed,
    IReadOnlyList<PushItemResult> Items
);
