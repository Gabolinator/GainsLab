using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;

using Microsoft.EntityFrameworkCore;

namespace GainsLab.Contracts.SyncService;

public class EquipmentSyncService : ISyncService<EquipmentSyncDto>
{
    private readonly GainLabPgDBContext _db;      // server-side
    private readonly ILogger<EquipmentSyncService> _log;

    public EquipmentSyncService(GainLabPgDBContext db, ILogger<EquipmentSyncService> log)
    {
        _db = db; _log = log;
    }


    public EntityType EntityType => EntityType.Equipment;
    public Task PushAsync(CancellationToken ct = default) { /* ... */ return Task.CompletedTask; }
   
    async Task<SyncPage<ISyncDto>> ISyncService.PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var page = await PullAsync(cur, take, ct); // generic one
        return new SyncPage<ISyncDto>(page.Time, page.Next , page.Items.Cast<ISyncDto>().ToList());
    }

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