using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;

using Microsoft.EntityFrameworkCore;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Provides read-side synchronization operations for equipment entities stored in the PostgreSQL database.
/// </summary>
public class EquipmentSyncService : ISyncService<EquipmentSyncDto>
{
    private readonly GainLabPgDBContext _db;      // server-side
    private readonly GainsLab.Core.Models.Core.Utilities.Logging.ILogger _log;

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
    public Task PushAsync(CancellationToken ct = default) { /* ... */ return Task.CompletedTask; }
   
    // async Task<SyncPage<ISyncDto>> ISyncService.PullAsync(SyncCursor cur, int take, CancellationToken ct)
    // {
    //     var page = await PullAsync(cur, take, ct); // generic one
    //     return new SyncPage<ISyncDto>(page.Time, page.Next , page.Items.Cast<ISyncDto>().ToList());
    // }
    //
    //
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

        var items = await q.Select(e => new EquipmentSyncDto(
           e.Iguid,
            e.Name,
           e.Descriptor == null ? null : e.Descriptor.GUID,
           e.UpdatedAtUtc,
           e.UpdatedSeq,
          e.IsDeleted   // include tombstones if you support soft delete
        )).ToListAsync(ct);

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<EquipmentSyncDto>(serverTime, next, items);
    }
}
