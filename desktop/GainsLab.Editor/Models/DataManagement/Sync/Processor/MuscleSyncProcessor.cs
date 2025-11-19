using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.Sync.Processor;

/// <summary>
/// Applies muscle sync payloads to the local SQLite store, including antagonist relationships.
/// </summary>
public class MuscleSyncProcessor : ISyncEntityProcessor
{
    private const string SyncActor = "sync";

    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IDescriptorResolver _descriptorResolver;
    
    public EntityType EntityType => EntityType.Muscle;
    
    public MuscleSyncProcessor(IDbContextFactory<GainLabSQLDBContext> dbContextFactory, IDescriptorResolver descriptorResolver ,ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _descriptorResolver = descriptorResolver;
    }
    
    /// <summary>
    /// Materializes the supplied muscle DTOs into the local database and syncs antagonist join rows.
    /// </summary>
    public async Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct)
    {
        _logger?.Log(nameof(MuscleSyncProcessor), $"Applying Async for {items.Count} items");

        if (items.Count == 0) return Result.SuccessResult();

        var typed = items.OfType<MuscleSyncDTO>().ToList();
        if (typed.Count == 0) return Result.SuccessResult();

        try
        {
            _logger?.Log(nameof(MuscleSyncProcessor), $"Applying Async for {items.Count} {nameof(MuscleSyncDTO)}");

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var descriptorCache = new Dictionary<Guid, DescriptorDTO>();
            var muscleLookup = new Dictionary<Guid, MuscleDTO>();
            var pendingAntagonists = new Dictionary<Guid, IReadOnlyList<Guid>>();

            foreach (var dto in typed)
            {
                ct.ThrowIfCancellationRequested();

                _logger?.Log(nameof(MuscleSyncProcessor),
                    $"Applying Async for {nameof(MuscleSyncDTO)} : {dto.Name} | {dto.GUID} | {(dto.DescriptorGUID == null ? "null descriptor guid" : dto.DescriptorGUID)}");
                
                var descriptor = await _descriptorResolver.ResolveDescriptorAsync(dbContext, dto.DescriptorGUID, descriptorCache, ct)
                    .ConfigureAwait(false);

                var entity = await dbContext.Muscles
                    .Include(e => e.Descriptor)
                    .FirstOrDefaultAsync(e => e.GUID == dto.GUID, ct)
                    .ConfigureAwait(false);

                if (entity is null)
                {
                    entity = new MuscleDTO
                    {
                        Name = dto.Name,
                        GUID = dto.GUID,
                        CreatedAtUtc = dto.UpdatedAtUtc,
                        CreatedBy = SyncActor,
                        Authority = dto.Authority
                    };

                    await dbContext.Muscles.AddAsync(entity, ct).ConfigureAwait(false);
                }

                entity.Name = dto.Name;
                entity.BodySection = dto.BodySection;
                entity.Descriptor = descriptor;
                entity.Authority = dto.Authority;

                entity.UpdatedAtUtc = dto.UpdatedAtUtc;
                entity.UpdatedSeq = dto.UpdatedSeq;
                entity.UpdatedBy = SyncActor;
                entity.Version = dto.UpdatedSeq;
                entity.IsDeleted = dto.IsDeleted;
                entity.DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null;
                entity.DeletedBy = dto.IsDeleted ? SyncActor : null;

                _logger?.Log(nameof(MuscleSyncProcessor), $"Prepared muscle {entity.Name} ({entity.GUID})");

                muscleLookup[dto.GUID] = entity;
                pendingAntagonists[dto.GUID] = NormalizeAntagonistGuids(dto);
            }

            _logger?.Log(nameof(MuscleSyncProcessor), "Save Changes Async (muscles)");
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            _logger?.Log(nameof(MuscleSyncProcessor), "Save Changes Async Completed (muscles)");

            foreach (var (muscleGuid, desiredAntagonists) in pendingAntagonists)
            {
                if (!muscleLookup.TryGetValue(muscleGuid, out var muscleEntity))
                    continue;

                await SyncAntagonistsAsync(dbContext, muscleEntity, desiredAntagonists, ct)
                    .ConfigureAwait(false);
            }

            _logger?.Log(nameof(MuscleSyncProcessor), "Save Changes Async (antagonists)");
            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            _logger?.Log(nameof(MuscleSyncProcessor), "Save Changes Async Completed (antagonists)");

            return Result.SuccessResult();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure("Sync cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(nameof(MuscleSyncProcessor), $"Failed to apply muscle sync: {ex.Message}");
            return Result.Failure($"Failed to apply muscle sync: {ex.GetBaseException().Message}");
        }
    }

    

    /// <summary>
    /// Normalizes the antagonist GUID payload into a deduped list, enforcing empty sets for tombstones.
    /// </summary>
    private static IReadOnlyList<Guid> NormalizeAntagonistGuids(MuscleSyncDTO dto)
    {
        if (dto.IsDeleted)
            return Array.Empty<Guid>();

        return dto.AntagonistGuids?
            .Where(g => g != Guid.Empty)
            .Distinct()
            .ToList()
            ?? new();
    }

    /// <summary>
    /// Adds, updates, or removes antagonist join rows so they match the desired GUID set.
    /// </summary>
    private async Task SyncAntagonistsAsync(
        GainLabSQLDBContext dbContext,
        MuscleDTO muscle,
        IReadOnlyList<Guid> desiredGuids,
        CancellationToken ct)
    {
        if (muscle.Id == 0)
        {
            _logger?.LogWarning(nameof(MuscleSyncProcessor),
                $"Skipping antagonist sync for {muscle.GUID} because it has no primary key yet.");
            return;
        }

        desiredGuids ??= Array.Empty<Guid>();
        var desiredSet = desiredGuids.Where(g => g != Guid.Empty).ToHashSet();

        var existingLinks = await dbContext.MuscleAntagonists
            .Where(link => link.MuscleId == muscle.Id)
            .Include(link => link.Antagonist)
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var toRemove = existingLinks
            .Where(link => link.Antagonist == null || !desiredSet.Contains(link.Antagonist.GUID))
            .ToList();

        if (toRemove.Count > 0)
        {
            dbContext.MuscleAntagonists.RemoveRange(toRemove);
        }

        var existingGuids = existingLinks
            .Where(link => link.Antagonist != null)
            .Select(link => link.Antagonist!.GUID)
            .ToHashSet();

        var missingGuids = desiredSet.Except(existingGuids).ToList();
        if (missingGuids.Count == 0)
            return;

        var related = await dbContext.Muscles
            .Where(m => missingGuids.Contains(m.GUID))
            .Select(m => new { m.GUID, m.Id })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var lookup = related.ToDictionary(x => x.GUID, x => x.Id);

        foreach (var guid in missingGuids)
        {
            if (!lookup.TryGetValue(guid, out var antagonistId))
            {
                _logger?.LogWarning(nameof(MuscleSyncProcessor),
                    $"Antagonist {guid} missing locally. Relationship for muscle {muscle.GUID} skipped.");
                continue;
            }

            dbContext.MuscleAntagonists.Add(new MuscleAntagonistDTO
            {
                MuscleId = muscle.Id,
                AntagonistId = antagonistId
            });
        }
    }
}
