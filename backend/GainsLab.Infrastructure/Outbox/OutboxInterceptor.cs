using System.Buffers;
using System.Text;
using System.Text.Json;
using GainsLab.Application.DTOs;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Outbox;
using GainsLab.Infrastructure.SyncService.Mapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;


namespace GainsLab.Infrastructure.Outbox;

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
    private static readonly HashSet<string> DedupIgnoredProperties =
        new(StringComparer.OrdinalIgnoreCase)
        {
            "Iid",
            "UpdatedAtUtc",
            "UpdatedSeq",
            "UpdatedBy",
            "CreatedAtUtc",
            "CreatedBy",
            "DeletedAt",
            "DeletedBy",
            "RowVersion",
            "Version"
        };

    /// <summary>
    /// Initializes a new instance of the <see cref="OutboxInterceptor"/> class.
    /// </summary>
    /// <param name="logger">Logger used to emit diagnostic details.</param>
    public OutboxInterceptor(ILogger logger) => _logger = logger;

    /// <inheritdoc />
    public override async ValueTask<InterceptionResult<int>> SavingChangesAsync(
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
            return await base.SavingChangesAsync(eventData, result, ct);
        }


        if (!_saveEmitted.TryGetValue(saveId, out var emitted))
        {
            emitted = new HashSet<(string, Guid, int)>();
            _saveEmitted[saveId] = emitted;
        }

        var entries = ctx.ChangeTracker.Entries<BaseRecord>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        var envelopes = new List<OutboxChangeRecord>();

        foreach (var e in entries)
        {
            var changeType = e.Entity.IsDeleted ? ChangeType.Delete
                              : e.State == EntityState.Added ? ChangeType.Insert
                              : ChangeType.Update;

            _logger?.Log(nameof(OutboxInterceptor),
                $"Intercepting saved changes on entry {e.Entity} - with id: {e.Entity.Iid} ChangeType: {changeType}");

            var key = (e.Entity.GetType().Name, e.Entity.Iguid, (int)changeType);

            var requiresPersistedId = e.State is EntityState.Modified or EntityState.Deleted;
            if (requiresPersistedId && e.Entity.Iid <= 0)
            {
                _logger?.LogWarning(nameof(OutboxInterceptor), $"Invalid entity ID (primary key) {e.Entity.Iid} for {e.Entity.Type}");
                continue;
            }

            if (emitted.Add(key)) // only once per *save*
            {
                if (!TrySerializeSyncPayload(e.Entity, out var payloadJson))
                {
                    _logger?.LogWarning(nameof(OutboxInterceptor),
                        $"Unable to serialize sync payload for {e.Entity.Type}. Entry skipped.");
                    continue;
                }

                var normalizedPayload = NormalizePayloadForDedup(payloadJson);
                if (await HasDuplicateOutboxEntryAsync(ctx, key.Item1, key.Item2, normalizedPayload, changeType, ct))
                {
                    _logger?.LogWarning(nameof(OutboxInterceptor),
                        $"Duplicate outbox entry ignored for {key.Item1} ({key.Item2}) change {changeType}.");
                    continue;
                }

                envelopes.Add(new OutboxChangeRecord
                {
                    Entity = key.Item1,
                    EntityGuid = key.Item2,
                    ChangeType = changeType,
                    PayloadJson = payloadJson
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

        return await base.SavingChangesAsync(eventData, result, ct);
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

    private static async Task<bool> HasDuplicateOutboxEntryAsync(
        GainLabSQLDBContext context,
        string entity,
        Guid entityGuid,
        string normalizedPayloadJson,
        ChangeType changeType,
        CancellationToken ct)
    {
        var candidates = await context.OutboxChanges
            .AsNoTracking()
            .Where(o =>
                o.Entity == entity &&
                o.EntityGuid == entityGuid &&
                o.ChangeType == changeType)
            .Select(o => o.PayloadJson)
            .ToListAsync(ct);

        foreach (var candidate in candidates)
        {
           if (NormalizePayloadForDedup(candidate) == normalizedPayloadJson)
                return true;
        }

        return false;
    }

    private static string NormalizePayloadForDedup(string payloadJson)
    {
        if (string.IsNullOrWhiteSpace(payloadJson))
            return string.Empty;

        try
        {
            using var doc = JsonDocument.Parse(payloadJson);
            var buffer = new ArrayBufferWriter<byte>();
            using (var writer = new Utf8JsonWriter(buffer))
            {
                writer.WriteStartObject();
                var orderedProperties = doc.RootElement
                    .EnumerateObject()
                    .Where(property => !DedupIgnoredProperties.Contains(property.Name))
                    .OrderBy(property => property.Name, StringComparer.OrdinalIgnoreCase);

                foreach (var property in orderedProperties)
                {
                    property.WriteTo(writer);
                }

                writer.WriteEndObject();
            }

            return Encoding.UTF8.GetString(buffer.WrittenSpan);
        }
        catch (JsonException)
        {
            // If payload is invalid JSON, fall back to raw string to avoid blocking persistence.
            return payloadJson;
        }
    }

    private static bool TrySerializeSyncPayload(BaseRecord entity, out string payloadJson)
    {
        payloadJson = string.Empty;
        if (!TryConvertToSyncRecord(entity, out var syncRecord))
            return false;

        payloadJson = JsonSerializer.Serialize(syncRecord, syncRecord.GetType());
        return true;
    }

    private static bool TryConvertToSyncRecord(BaseRecord entity, out ISyncDto? syncRecord)
    {
        syncRecord = null;

        switch (entity.Type)
        {
            case EntityType.Descriptor when entity is DescriptorRecord descriptor:
                syncRecord = DescriptorSyncMapper.ToSyncDTO(descriptor);
                return true;
            case EntityType.Equipment when entity is EquipmentRecord equipment:
                syncRecord = EquipmentSyncMapper.ToSyncDTO(equipment);
                return true;
            case EntityType.Muscle when entity is MuscleRecord muscle:
                syncRecord = MuscleSyncMapper.ToSyncDTO(muscle);
                return true;
            default:
                return false;
        }
    }
}
