using System.Data;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Contracts.SyncService.Mapper;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Provides read-side synchronization operations for equipment entities stored in the PostgreSQL database.
/// </summary>
public class EquipmentSyncService : ISyncService<EquipmentSyncDTO>
{
    private readonly GainLabPgDBContext _db;      // server-side
    private readonly GainsLab.Core.Models.Core.Utilities.Logging.ILogger _log;
    private const string SyncActor = "sync";
    private static readonly Guid PlaceholderDescriptorGuid = Guid.Empty;
    private readonly Dictionary<Guid, DescriptorDTO> _descriptorCache = new();
    
    /// <summary>
    /// Gets the DTO type handled by this service.
    /// </summary>
    Type ISyncService.DtoType => typeof(EquipmentSyncDTO);
    
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
        => PushAsync(dtos.Cast<EquipmentSyncDTO>(), ct);

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
    public async Task<SyncPage<EquipmentSyncDTO>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
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

        return new SyncPage<EquipmentSyncDTO>(serverTime, next, items);
    }

    /// <summary>
    /// Processes equipment mutations pushed from clients.
    /// </summary>
    /// <param name="items">Incoming equipment payloads.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A push result describing how each item was handled.</returns>
    public async Task<PushResult> PushAsync(IEnumerable<EquipmentSyncDTO> items, CancellationToken ct)
    {
        _descriptorCache.Clear();
        var now = DateTimeOffset.UtcNow;
        var preValidationResults = new List<PushItemResult>();
        var preValidationFailures = 0;

        // Map once (don’t trust client timestamps/seq; we’ll stamp below)
        var payloads = new List<EquipmentDTO>();
        foreach (var dto in items)
        {
            _log.Log(nameof(EquipmentSyncService),$"trying to push {dto.Name} - {dto.GUID}");
            
            if (dto.GUID == Guid.Empty)
            {
                const string reason = "Equipment GUID is required.";
                _log.LogError(nameof(EquipmentSyncService),
                    $"Push item {dto.GUID} rejected before persistence: {reason}");
                preValidationResults.Add(
                    new PushItemResult(Guid.Empty, PushItemStatus.Failed, reason));
                preValidationFailures++;
                continue;
            }

            //get the descriptor here using the descriptors guid
            DescriptorDTO descriptorDto = GetDescriptorDto(dto.DescriptorGUID);
            
            payloads.Add(EquipmentSyncMapper.FromSyncDTO(dto,  descriptorDto, SyncActor));
        }

        var executionStrategy = _db.Database.CreateExecutionStrategy();
        PushResult? finalResult = null;

        try
        {
            await executionStrategy.ExecuteAsync(async () =>
            {
                var txResults = new List<PushItemResult>();
                var accepted = 0;
                var failed = 0;

                await using var tx = await _db.Database.BeginTransactionAsync(ct);
                try
                {
                    foreach (var incoming in payloads)
                    {
                        try
                        {
                            var existing = await _db.Equipments
                                .Include(e => e.Descriptor)
                                .SingleOrDefaultAsync(e => e.GUID == incoming.GUID, ct);

                            if (existing is null)
                            {
                                if (incoming.IsDeleted)
                                {
                                    txResults.Add(new PushItemResult(incoming.Iguid, PushItemStatus.NotFound,
                                        "Delete ignored for missing row."));
                                    continue;
                                }

                                incoming.UpdatedAtUtc = now;
                                incoming.UpdatedSeq = await NextUpdateSeqAsync(ct);

                                await _db.Equipments.AddAsync(incoming, ct);
                                txResults.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Upserted));
                                accepted++;
                            }
                            else
                            {
                                if (existing.Authority == DataAuthority.Upstream)
                                {
                                    txResults.Add(new PushItemResult(incoming.Iguid,
                                        PushItemStatus.Failed,
                                        "Upstream-owned equipment cannot be modified downstream."));
                                    continue;
                                }

                                var isIncomingNewer =
                                    incoming.UpdatedAtUtc > existing.UpdatedAtUtc ||
                                    (incoming.UpdatedAtUtc == existing.UpdatedAtUtc &&
                                     incoming.UpdatedSeq > existing.UpdatedSeq);

                                if (!isIncomingNewer)
                                {
                                    txResults.Add(new PushItemResult(incoming.Iguid,
                                        PushItemStatus.SkippedDuplicate,
                                        "Incoming not newer."));
                                    continue;
                                }

                                if (incoming.IsDeleted)
                                {
                                    existing.IsDeleted = true;
                                    existing.UpdatedAtUtc = now;
                                    existing.UpdatedSeq = await NextUpdateSeqAsync(ct);

                                    _db.Equipments.Update(existing);
                                    txResults.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Deleted));
                                    accepted++;
                                }
                                else
                                {
                                    existing.Name = incoming.Name;
                                    existing.Descriptor = incoming.Descriptor;
                                    existing.IsDeleted = false;

                                    existing.UpdatedAtUtc = now;
                                    existing.UpdatedSeq = await NextUpdateSeqAsync(ct);

                                    _db.Equipments.Update(existing);
                                    txResults.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Upserted));
                                    accepted++;
                                }
                            }
                        }
                        catch (DbUpdateException ex)
                        {
                            _log.LogError(nameof(EquipmentSyncService),
                                $"Push item {incoming.Iguid} failed: {ex.GetBaseException().Message}");
                            txResults.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed,
                                ex.GetBaseException().Message));
                            failed++;
                        }
                        catch (Exception ex)
                        {
                            _log.LogError(nameof(EquipmentSyncService), $"Push item {incoming.Iguid} failed: {ex}");
                            txResults.Add(new PushItemResult(incoming.Iguid, PushItemStatus.Failed, ex.Message));
                            failed++;
                        }
                    }

                    await _db.SaveChangesAsync(ct);
                    await tx.CommitAsync(ct);

                    finalResult = new PushResult(
                        now,
                        accepted,
                        preValidationFailures + failed,
                        preValidationResults.Concat(txResults).ToList());
                }
                catch
                {
                    await tx.RollbackAsync(ct);
                    throw;
                }
            });
        }
        catch (Exception ex)
        {
            _log.LogError(nameof(EquipmentSyncService), $"Push batch failed/rolled back: {ex}");
            var transactionalFailures = payloads
                .Select(p => new PushItemResult(p.Iguid, PushItemStatus.Failed, "Batch rolled back."))
                .ToList();

            var combinedResults = preValidationResults.Concat(transactionalFailures).ToList();
            var totalFailed = preValidationFailures + payloads.Count;
            return new PushResult(now, 0, totalFailed, combinedResults);
        }

        return finalResult ?? new PushResult(now, 0, preValidationFailures, preValidationResults);
    }

    private DescriptorDTO GetDescriptorDto(Guid? descriptorGuid)
    {
        var cacheKey = descriptorGuid ?? PlaceholderDescriptorGuid;
        if (_descriptorCache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        DescriptorDTO descriptor;
        var missingDescriptor = !descriptorGuid.HasValue || descriptorGuid.Value == Guid.Empty;

        if (missingDescriptor)
        {
            descriptor = ResolvePlaceholderDescriptor();
        }
        else
        {
            var guid = descriptorGuid.Value;
            descriptor = TryGetTrackedDescriptor(guid)
                         ?? CreateDescriptorStub(guid, "none");

            if (descriptor.Id == 0)
            {
                _log.Log(nameof(EquipmentSyncService),
                    $"Descriptor {descriptorGuid} not found. Creating stub so FK constraint is satisfied.");
            }
        }

        _descriptorCache[cacheKey] = descriptor;
        return descriptor;
    }

    private DescriptorDTO ResolvePlaceholderDescriptor()
    {
        var descriptor = TryGetTrackedDescriptor(PlaceholderDescriptorGuid);
        if (descriptor != null)
        {
            return descriptor;
        }

        return CreateDescriptorStub(PlaceholderDescriptorGuid, "unspecified");
    }

    private DescriptorDTO? TryGetTrackedDescriptor(Guid guid)
    {
        return _db.Descriptors.Local.FirstOrDefault(d => d.GUID == guid)
               ?? _db.Descriptors.FirstOrDefault(d => d.GUID == guid);
    }

    private DescriptorDTO CreateDescriptorStub(Guid guid, string content)
    {
        var now = DateTimeOffset.UtcNow;
        return new DescriptorDTO
        {
            GUID = guid,
            Content = content,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = SyncActor,
            UpdatedBy = SyncActor,
            IsDeleted = false,
            Authority = DataAuthority.Bidirectional
        };
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
