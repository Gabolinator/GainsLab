using System.Text.Json;
using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Outbox;
using Microsoft.EntityFrameworkCore;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;

namespace GainsLab.Contracts.Outbox;

/// <summary>
/// Dispatches local outbox entries to the remote sync API and marks successful envelopes as sent.
/// </summary>
public sealed class OutboxDispatcher : IOutboxDispatcher
{
    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly HttpClient _httpClient;

    /// <summary>
    /// Creates an outbox dispatcher that uses the supplied services to read pending changes and reach the API.
    /// </summary>
    /// <param name="dbContextFactory">Factory used to create scoped database contexts.</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    /// <param name="httpClient">HTTP client configured to point at the sync API.</param>
    public OutboxDispatcher(
        IDbContextFactory<GainLabSQLDBContext> dbContextFactory,
        ILogger logger,
        HttpClient httpClient)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(CancellationToken ct)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var pending = await dbContext.Set<OutboxChangeDto>()
                .Where(o => !o.Sent)
                .OrderBy(o => o.OccurredAt)
                .Take(100)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (pending.Count == 0)
                return Result.SuccessResult();

            var requests = await BuildPushRequestsAsync(pending, ct).ConfigureAwait(false);

            foreach (var request in requests)
            {
                ct.ThrowIfCancellationRequested();
                await DispatchRequestAsync(request, ct).ConfigureAwait(false);
            }

            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.SuccessResult();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure("Outbox dispatch cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(OutboxDispatcher), $"Failed to dispatch outbox: {ex.Message}");
            return Result.Failure($"Failed to dispatch outbox: {ex.Message}");
        }
    }

    /// <summary>
    /// Builds HTTP push requests grouped by entity type from the pending outbox rows.
    /// </summary>
    /// <param name="pending">The outbox records that still need to be dispatched.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    private async Task<IReadOnlyList<PushRequest>> BuildPushRequestsAsync(
        IReadOnlyList<OutboxChangeDto> pending,
        CancellationToken ct)
    {
        var grouped = new Dictionary<EntityType, List<OutboxItem>>(pending.Count);

        foreach (var change in pending)
        {
            ct.ThrowIfCancellationRequested();

            if (!TryResolveEntityType(change, out var entityType))
            {
                _logger.LogWarning(nameof(OutboxDispatcher),
                    $"Skipping outbox item {change.Id} because the entity type could not be resolved.");
                continue;
            }

            using var doc = JsonDocument.Parse(change.PayloadJson);
            var payload = doc.RootElement.Clone();

            if (!grouped.TryGetValue(entityType, out var bucket))
            {
                bucket = new List<OutboxItem>();
                grouped[entityType] = bucket;
            }

            bucket.Add(new OutboxItem(change, payload));
        }

        var requests = new List<PushRequest>(grouped.Count);

        foreach (var kvp in grouped)
        {
            var payload = kvp.Value.Select(v => v.Payload).ToList();
            var envelope = new SyncPushEnvelope(DateTimeOffset.UtcNow, payload);
            requests.Add(new PushRequest(kvp.Key, envelope, kvp.Value));
        }

        return requests;
    }

    /// <summary>
    /// Issues the HTTP POST for a single push request and updates outbox flags based on the response.
    /// </summary>
    /// <param name="request">The grouped payload to dispatch.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    private async Task DispatchRequestAsync(PushRequest request, CancellationToken ct)
    {
        try
        {
            var entityRoute = request.EntityType.ToString();
            var response = await _httpClient.PostAsJsonAsync($"sync/{entityRoute}", request.Envelope, ct)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(nameof(OutboxDispatcher),
                    $"Push for {request.EntityType} failed with status {(int)response.StatusCode}.");
                return;
            }

            var result = await response.Content.ReadFromJsonAsync<PushResult>(cancellationToken: ct)
                .ConfigureAwait(false);

            if (result is null)
            {
                _logger.LogError(nameof(OutboxDispatcher),
                    $"Push for {request.EntityType} returned an empty payload.");
                return;
            }

            MarkDispatched(request.Items, result);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(OutboxDispatcher),
                $"Push for {request.EntityType} failed: {ex.GetBaseException().Message}");
        }
    }

    /// <summary>
    /// Marks outbox rows as sent when the server successfully processes them.
    /// </summary>
    /// <param name="items">The outbox items that were submitted.</param>
    /// <param name="pushResult">Server response describing the outcome of each item.</param>
    private void MarkDispatched(IReadOnlyList<OutboxItem> items, PushResult pushResult)
    {
        var resultLookup = pushResult.Items.ToDictionary(i => i.Id, i => i);

        foreach (var item in items)
        {
            if (!resultLookup.TryGetValue(item.Change.EntityGuid, out var outcome))
            {
                _logger.LogWarning(nameof(OutboxDispatcher),
                    $"Push result did not include entry for change {item.Change.EntityGuid}.");
                continue;
            }

            switch (outcome.Status)
            {
                case PushItemStatus.Upserted:
                case PushItemStatus.Deleted:
                case PushItemStatus.SkippedDuplicate:
                case PushItemStatus.NotFound:
                    item.Change.Sent = true;
                    break;
                case PushItemStatus.Conflict:
                case PushItemStatus.Failed:
                    _logger.LogWarning(nameof(OutboxDispatcher),
                        $"Server returned {outcome.Status} for change {item.Change.EntityGuid}: {outcome.Message}");
                    break;
            }
        }
    }

    /// <summary>
    /// Attempts to derive the <see cref="EntityType"/> from the serialized payload.
    /// </summary>
    /// <param name="change">The outbox change containing the serialized payload.</param>
    /// <param name="entityType">Populated with the resolved entity type when successful.</param>
    private bool TryResolveEntityType(OutboxChangeDto change, out EntityType entityType)
    {
        try
        {
            using var doc = JsonDocument.Parse(change.PayloadJson);
            if (!doc.RootElement.TryGetProperty("Type", out var typeProperty))
            {
                entityType = default;
                return false;
            }

            if (typeProperty.ValueKind == JsonValueKind.String &&
                Enum.TryParse<EntityType>(typeProperty.GetString(), true, out var fromString))
            {
                entityType = fromString;
                return true;
            }

            if (typeProperty.ValueKind == JsonValueKind.Number &&
                typeProperty.TryGetInt32(out var numeric))
            {
                entityType = (EntityType)numeric;
                return true;
            }

            entityType = default;
            return false;
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(OutboxDispatcher),
                $"Failed to resolve entity type for outbox item {change.Id}: {ex.GetBaseException().Message}");
            entityType = default;
            return false;
        }
    }

    /// <summary>
    /// Encapsulates a pending outbox change and its cloned payload.
    /// </summary>
    private sealed record OutboxItem(OutboxChangeDto Change, JsonElement Payload);

    /// <summary>
    /// Represents a pending HTTP submission grouped by entity type.
    /// </summary>
    private sealed record PushRequest(
        EntityType EntityType,
        SyncPushEnvelope Envelope,
        IReadOnlyList<OutboxItem> Items);
}
