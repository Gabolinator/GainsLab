using System.Text.Json;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Outbox;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using ILogger = GainsLab.Core.Models.Core.Utilities.Logging.ILogger;

namespace GainsLab.Contracts.Outbox;

/// <summary>
/// SaveChanges interceptor that captures entity mutations and persists outbox entries.
/// </summary>
public class OutboxInterceptor : SaveChangesInterceptor
{
    private readonly ILogger _logger;

    // Track active save cycles per DbContext instance
    private readonly HashSet<Guid> _activeSaves = new();
    // Dedup per *save*, not per interceptor lifetime
    private readonly Dictionary<Guid, HashSet<(string, Guid, int)>> _saveEmitted = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxInterceptor"/> class.
    /// </summary>
    /// <param name="logger">Logger used to emit diagnostic details.</param>
    public OutboxInterceptor(ILogger logger) => _logger = logger;

    /// <inheritdoc />
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData,
        InterceptionResult<int> result,
        CancellationToken ct = default)
    {
        var ctx = (GainLabSQLDBContext)eventData.Context!;
        var saveId = ctx.ContextId.InstanceId;

        _logger?.Log(nameof(OutboxInterceptor),
            $"Intercepting saved changes for instance {ctx.ContextId.InstanceId}");

        // If EF restarts the pipeline in the same SaveChanges call, skip the second pass
        if (!_activeSaves.Add(saveId))
        {
            _logger?.LogWarning(nameof(OutboxInterceptor),
                $"Already Intercepted saved changes for instance {ctx.ContextId.InstanceId}");
            return base.SavingChangesAsync(eventData, result, ct);
        }


        if (!_saveEmitted.TryGetValue(saveId, out var emitted))
        {
            emitted = new HashSet<(string, Guid, int)>();
            _saveEmitted[saveId] = emitted;
        }

        var entries = ctx.ChangeTracker.Entries<BaseDto>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var envelopes = new List<OutboxChangeDto>();

        foreach (var e in entries)
        {
            var changeType = e.Entity.IsDeleted ? ChangeType.Delete
                              : e.State == EntityState.Added ? ChangeType.Insert
                              : ChangeType.Update;

            _logger?.Log(nameof(OutboxInterceptor),
                $"Intercepting saved changes on entry {e.Entity} - ChangeType: {changeType}");

            var key = (e.Entity.GetType().Name, e.Entity.Iguid, (int)changeType);
            if (emitted.Add(key)) // only once per *save*
            {
                envelopes.Add(new OutboxChangeDto
                {
                    Entity = key.Item1,
                    EntityGuid = key.Item2,
                    ChangeType = changeType,
                    PayloadJson = JsonSerializer.Serialize(e.Entity)
                });
            }
        }

        if (envelopes.Count > 0)
        {
            // Add outbox rows; EF might restart the pipeline → we’re guarded above
            foreach (var o in envelopes)
                _logger?.Log(nameof(OutboxInterceptor),
                    $"OUTBOX → {o} len(JSON)={o.PayloadJson?.Length ?? 0}");
            
            ctx.AddRange(envelopes);
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }

    /// <inheritdoc />
    public override ValueTask<int> SavedChangesAsync(SaveChangesCompletedEventData eventData, int result, CancellationToken ct = default)
    {
        ClearPerSaveState(eventData);
        return base.SavedChangesAsync(eventData, result, ct);
    }

    /// <inheritdoc />
    public override Task SaveChangesFailedAsync(
        DbContextErrorEventData eventData,
        CancellationToken cancellationToken = default)
    {
        var ctx = (GainLabSQLDBContext?)eventData.Context;
        var saveId = ctx?.ContextId.InstanceId;
        _logger?.LogError(nameof(OutboxInterceptor),
            $"SaveChangesFailed for instance {saveId}: {eventData.Exception}");

        ClearPerSaveState(eventData);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Clears interception state associated with the given EF Core context.
    /// </summary>
    private void ClearPerSaveState(DbContextEventData eventData)
    {
        if (eventData.Context is null) return;
        var saveId = eventData.Context.ContextId.InstanceId;
        _logger?.Log(nameof(OutboxInterceptor),
            $"Cleared Save State for instance {saveId}");

        _activeSaves.Remove(saveId);
        _saveEmitted.Remove(saveId);
    }
}
