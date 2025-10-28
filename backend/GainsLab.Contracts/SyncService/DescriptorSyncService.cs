using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;

using Microsoft.EntityFrameworkCore;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Provides read-side synchronization operations for descriptor entities stored in the PostgreSQL database.
/// </summary>
public class DescriptorSyncService : ISyncService<DescriptorSyncDto>
{
    private readonly GainLabPgDBContext _db;
    private readonly Core.Models.Core.Utilities.Logging.ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DescriptorSyncService"/> class.
    /// </summary>
    public DescriptorSyncService(GainLabPgDBContext db, Core.Models.Core.Utilities.Logging.ILogger log)
    {
        _db = db;
        _logger = log;
    }

    /// <inheritdoc />
    public EntityType EntityType => EntityType.Descriptor;

    /// <inheritdoc />
    public Task PushAsync(CancellationToken ct = default) => Task.CompletedTask;

    /// <inheritdoc />
    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct); // returns SyncPage<DescriptorSyncDto>

    /// <summary>
    /// Retrieves a page of descriptor changes newer than the supplied cursor, ordered deterministically for incremental sync.
    /// </summary>
    /// <param name="cur">The cursor that represents the last successful sync position.</param>
    /// <param name="take">Maximum number of records to include in the page.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A <see cref="SyncPage{TSyncDto}"/> containing descriptor DTOs and the next cursor when available.</returns>
    public async Task<SyncPage<DescriptorSyncDto>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        var serverTime = DateTimeOffset.UtcNow;
        take = Math.Clamp(take, 1, 500);

        var q = _db.Descriptors.AsNoTracking()
            .Where(d => d.UpdatedAtUtc > cur.Ts
                        || (d.UpdatedAtUtc == cur.Ts && d.UpdatedSeq > cur.Seq))
            .OrderBy(d => d.UpdatedAtUtc)
            .ThenBy(d => d.UpdatedSeq)
            .Take(take);

        var items = await q.Select(d => new DescriptorSyncDto(
            d.GUID,
            d.Content,
            d.UpdatedAtUtc,
            d.UpdatedSeq,
            d.IsDeleted
        )).ToListAsync(ct);

        _logger.Log(nameof(DescriptorSyncService), $"Pull Descriptor Async- take {take} - items count: {items.Count} items[0] {(items.Count > 0 ? items[0] : "none")} ");

        SyncCursor? next = items.Count < take
            ? null
            : new SyncCursor(items[^1].UpdatedAtUtc, items[^1].UpdatedSeq);

        return new SyncPage<DescriptorSyncDto>(serverTime, next, items);
    }
}
