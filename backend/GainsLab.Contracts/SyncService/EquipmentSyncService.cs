using System.Data;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Provides read-side synchronization operations for equipment entities stored in the PostgreSQL database.
/// </summary>
public class EquipmentSyncService : ISyncService<EquipmentSyncDto>
{
    private readonly GainLabPgDBContext _db;      // server-side
    private readonly GainsLab.Core.Models.Core.Utilities.Logging.ILogger _log;
    private const string SyncActor = "sync";
    
    /// <summary>
    /// Gets the DTO type handled by this service.
    /// </summary>
    Type ISyncService.DtoType => typeof(EquipmentSyncDto);
    
    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentSyncService"/> class.
    /// </summary>
    public EquipmentSyncService(GainLabPgDBContext db, GainsLab.Core.Models.Core.Utilities.Logging.ILogger log)
    {
        _db = db; _log = log;
    }

    /// <inheritdoc />
    public EntityType EntityType => EntityType.Equipment;

    /// <inheritdoc />
    Task<PushResult> ISyncService.PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct)
        => PushAsync(dtos.Cast<EquipmentSyncDto>(), ct);

    /// <inheritdoc />
    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct); // returns SyncPage<EquipmentSyncDto>

    /// <summary>
    /// Retrieves a page of equipment changes newer than the supplied cursor, ordered deterministically for incremental sync.
    /// </summary>
    /// <param name="cur">The cursor that represents the last successful sync position.</param>
    /// <param name="take">Maximum number of records to include in the page.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A <see cref="SyncPage{TSyncDto}"/> containing equipment DTOs and the next cursor when available.</returns>
    public async Task<SyncPage<EquipmentSyncDto>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var serverTime = DateTimeOffset.UtcNow;

        var q = _db.Equipments.AsNoTracking()
            .Include(e => e.Descriptor)
            .Where(e => e.UpdatedAtUtc > cur.Ts
                        || (e.UpdatedAtUtc == cur.Ts && e.UpdatedSeq > cur.Seq))
            .OrderBy(e => e.UpdatedAtUtc)
            .ThenBy(e => e.UpdatedSeq)
            .Take(Math.Clamp(take, 1, 500));

        var items = await q.Select(e => EquipmentSyncMapper.ToSyncDTO(e)  // include tombstones if you support soft delete
        ).ToListAsync(ct);

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<EquipmentSyncDto>(serverTime, next, items);
    }

    /// <summary>
    /// Processes equipment mutations pushed from clients.
    /// </summary>
    /// <param name="items">Incoming equipment payloads.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A push result describing how each item was handled.</returns>
    public async Task<PushResult> PushAsync(IEnumerable<EquipmentSyncDto> items, CancellationToken ct)
    {
        var now = DateTimeOffset.UtcNow;
        var results = new List<PushItemResult>();
        var accepted = 0;
        var failed = 0;

        // map once (don’t trust client timestamps/seq; we’ll stamp below)
        var mapped = items.Select(s => EquipmentSyncMapper.FromSyncDTO(s, descriptor: null,SyncActor)).ToList();

        await using var tx = await _db.Database.BeginTransactionAsync(ct);
        try
        {
            foreach (var incoming in mapped)
            {
                try
                {
                    // 1) Resolve descriptor FK (TODO in your code)
                    // incoming.Descriptor = await ResolveDescriptorIdOrNullAsync(incoming.DescriptorGuid, ct);

                    var existing = await _db.Equipments
                        .FirstOrDefaultAsync(e=> e.Equals(incoming), ct);

                    if (existing is null)
                    {
                        if (incoming.IsDeleted)
                        {
                            // Delete for missing row: no-op
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.NotFound,
                                "Delete ignored for missing row."));
                            continue;
                        }

                        // New row
                        incoming.UpdatedAtUtc = now;
                        incoming.UpdatedSeq = await NextUpdateSeqAsync(ct);

                        await _db.Equipments.AddAsync(incoming, ct);
                        results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Upserted));
                        accepted++;
                    }
                    else
                    {
                        // 2) Conflict/ordering check using client values to decide, but overwrite with server stamps
                        var isIncomingNewer =
                            incoming.UpdatedAtUtc > existing.UpdatedAtUtc ||
                            (incoming.UpdatedAtUtc == existing.UpdatedAtUtc &&
                             incoming.UpdatedSeq > existing.UpdatedSeq);

                        if (!isIncomingNewer)
                        {
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.SkippedDuplicate,
                                "Incoming not newer."));
                            continue;
                        }

                        if (incoming.IsDeleted)
                        {
                            existing.IsDeleted = true;
                            existing.UpdatedAtUtc = now;
                            existing.UpdatedSeq = await NextUpdateSeqAsync(ct);

                            _db.Equipments.Update(existing);
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Deleted));
                            accepted++;
                        }
                        else
                        {
                            // Update mutable fields only
                            existing.Name = incoming.Name;
                            existing.Descriptor = incoming.Descriptor;
                            existing.IsDeleted = false;

                            existing.UpdatedAtUtc = now;
                            existing.UpdatedSeq = await NextUpdateSeqAsync(ct);

                            _db.Equipments.Update(existing);
                            results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Upserted));
                            accepted++;
                        }
                    }
                }
                catch (DbUpdateException ex)
                {
                    // Likely constraint/duplicate/fk violations
                    _log.LogError(nameof(EquipmentSyncService),
                        $"Push item {incoming.Iguid} failed: {ex.GetBaseException().Message}");
                    results.Add(
                        new PushItemResult(incoming.Iguid, PushItemStatus.Failed, ex.GetBaseException().Message));
                    failed++;
                }
                catch (Exception ex)
                {
                    _log.LogError(nameof(EquipmentSyncService), $"Push item {incoming.Iguid} failed: {ex}");
                    results.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed, ex.Message));
                    failed++;
                }
            }

            // 3) Persist once
            await _db.SaveChangesAsync(ct);
            await tx.CommitAsync(ct);

            return new PushResult(now, accepted, failed, results);
        }
        catch (Exception ex)
        {
            await tx.RollbackAsync(ct);
            _log.LogError(nameof(EquipmentSyncService), $"Push batch failed/rolled back: {ex}");
            // Mark everything as failed (caller can retry)
            var allFailed =
                mapped.Select(m => new PushItemResult(m.Iguid, PushItemStatus.Failed, "Batch rolled back."));
            return new PushResult(now, 0, mapped.Count, allFailed.ToList());
        }
    }
    
    /// <summary>
    /// Obtains the next sequence number used to order equipment updates.
    /// </summary>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    private async Task<long> NextUpdateSeqAsync(CancellationToken ct)
    {
        // Postgres: SELECT nextval('update_seq');
        await using var cmd = _db.Database.GetDbConnection().CreateCommand();
        cmd.CommandText = "SELECT nextval('update_seq')";
        if (cmd.Connection!.State != ConnectionState.Open)
            await cmd.Connection.OpenAsync(ct);
        var val = await cmd.ExecuteScalarAsync(ct);
        return Convert.ToInt64(val);
    }
    
}
