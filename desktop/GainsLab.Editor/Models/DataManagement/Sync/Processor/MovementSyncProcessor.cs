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

public class MovementSyncProcessor : ISyncEntityProcessor
{
    private const string SyncActor = "sync";

    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IDescriptorResolver _descriptorResolver;

    public EntityType EntityType => EntityType.Movement;
    
    public MovementSyncProcessor(IDbContextFactory<GainLabSQLDBContext> dbContextFactory, IDescriptorResolver descriptorResolver ,ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _descriptorResolver = descriptorResolver;
    }
    
    public async Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct)
    {
         _logger?.Log(nameof(MuscleSyncProcessor), $"Applying Async for {items.Count} items");

        if (items.Count == 0) return Result.SuccessResult();

        var typed = items.OfType<MovementSyncDTO>().ToList();
        if (typed.Count == 0) return Result.SuccessResult();

        try
        {
            _logger?.Log(nameof(MovementSyncProcessor), $"Applying Async for {items.Count} {nameof(MovementSyncDTO)}");

            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
            var descriptorCache = new Dictionary<Guid, DescriptorDTO>();
            var movementLookup = new Dictionary<Guid, MovementDTO>();
            var muscleLookup = new Dictionary<Guid, MuscleDTO>();
            var equipmentLookup = new Dictionary<Guid, EquipmentDTO>();
            
            foreach (var dto in typed)
            {
                ct.ThrowIfCancellationRequested();

                _logger?.Log(nameof(MuscleSyncProcessor),
                    $"Applying Async for {nameof(MovementSyncDTO)} : {dto.Name} | {dto.GUID} | {(dto.DescriptorGUID == null ? "null descriptor guid" : dto.DescriptorGUID)}");
                
                var descriptor = await _descriptorResolver.ResolveDescriptorAsync(dbContext, dto.DescriptorGUID, descriptorCache, ct)
                    .ConfigureAwait(false);

                var entity = await dbContext.Movement
                    .Include(e => e.Descriptor)
                    .FirstOrDefaultAsync(e => e.GUID == dto.GUID, ct)
                    .ConfigureAwait(false);

                if (entity is null)
                {
                    entity = new MovementDTO
                    {
                        Name = dto.Name,
                        GUID = dto.GUID,
                        CreatedAtUtc = dto.UpdatedAtUtc,
                        CreatedBy = SyncActor,
                        Authority = dto.Authority
                    };

                    await dbContext.Movement.AddAsync(entity, ct).ConfigureAwait(false);
                }

                entity.Name = dto.Name;
                entity.Descriptor = descriptor;
                entity.Authority = dto.Authority;
                entity.UpdatedAtUtc = dto.UpdatedAtUtc;
                entity.UpdatedSeq = dto.UpdatedSeq;
                entity.UpdatedBy = SyncActor;
                entity.Version = dto.UpdatedSeq;
                entity.IsDeleted = dto.IsDeleted;
                entity.DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null;
                entity.DeletedBy = dto.IsDeleted ? SyncActor : null;

                _logger?.Log(nameof(MovementSyncProcessor), $"Prepared muscle {entity.Name} ({entity.GUID})");

                movementLookup[dto.GUID] = entity;
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
            _logger?.LogError(nameof(MovementSyncProcessor), $"Failed to apply muscle sync: {ex.Message}");
            return Result.Failure($"Failed to apply muscle sync: {ex.GetBaseException().Message}");
        }
    }
}