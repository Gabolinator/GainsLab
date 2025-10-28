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
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.Sync;

/// <summary>
/// Applies equipment sync payloads to the local SQLite database.
/// </summary>
public sealed class EquipmentSyncProcessor : ISyncEntityProcessor
{
    private const string SyncActor = "sync";

    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Creates a processor that materializes remote equipment payloads into SQLite entities.
    /// </summary>
    public EquipmentSyncProcessor(IDbContextFactory<GainLabSQLDBContext> dbContextFactory, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public EntityType EntityType => EntityType.Equipment;

    /// <inheritdoc />
public async Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct)
{
    _logger?.Log(nameof(EquipmentSyncProcessor), $"Applying Async for {items.Count} items");

    
    if (items.Count == 0) return Result.SuccessResult();

    var typed = items.OfType<EquipmentSyncDto>().ToList();
    if (typed.Count == 0) return Result.SuccessResult();

    try
    {
        
        _logger?.Log(nameof(EquipmentSyncProcessor), $"Applying Async for {items.Count} {nameof(EquipmentSyncDto)}");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var descriptorCache = new Dictionary<Guid, DescriptorDTO>();

        
        
        foreach (var dto in typed)
        {
            ct.ThrowIfCancellationRequested();

            // Resolve without saving here
            var descriptor = await ResolveDescriptorAsync(dbContext, dto.DescriptorGUID, descriptorCache, ct)
                .ConfigureAwait(false);

            var entity = await dbContext.Equipments
                .Include(e => e.Descriptor)
                .FirstOrDefaultAsync(e => e.GUID == dto.GUID, ct)
                .ConfigureAwait(false);

            if (entity is null)
            {
                entity = new EquipmentDTO
                {
                    Name = dto.Name,
                    GUID = dto.GUID,
                    CreatedAtUtc = dto.UpdatedAtUtc,
                    CreatedBy = SyncActor,
                };

                await dbContext.Equipments.AddAsync(entity, ct).ConfigureAwait(false);
            }

            entity.Name = dto.Name;

            // Set the relationship; EF will handle DescriptorID
            entity.Descriptor = descriptor;

            entity.UpdatedAtUtc = dto.UpdatedAtUtc;
            entity.UpdatedSeq = dto.UpdatedSeq;
            entity.UpdatedBy = SyncActor;
            entity.Version = dto.UpdatedSeq;
            entity.IsDeleted = dto.IsDeleted;
            entity.DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null;
            entity.DeletedBy = dto.IsDeleted ? SyncActor : null;
            
            _logger?.Log(nameof(EquipmentSyncProcessor), $"Applying Async for {entity}");

        }

        // SINGLE save per batch
        _logger?.Log(nameof(EquipmentSyncProcessor), $"Save Changes Async");
        await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
        _logger?.Log(nameof(EquipmentSyncProcessor), $"Save Changes Async Completed");

        return Result.SuccessResult();
    }
    catch (OperationCanceledException)
    {
        return Result.Failure("Sync cancelled");
    }
    catch (Exception ex)
    {
        _logger?.LogError(nameof(EquipmentSyncProcessor), $"Failed to apply equipment sync: {ex.Message}");
        return Result.Failure($"Failed to apply equipment sync: {ex.GetBaseException().Message}");
    }
}

private async Task<DescriptorDTO> ResolveDescriptorAsync(
    GainLabSQLDBContext dbContext,
    Guid? descriptorGuid,
    IDictionary<Guid, DescriptorDTO> cache,
    CancellationToken ct)
{
    if (descriptorGuid is null)
        throw new ArgumentNullException(nameof(descriptorGuid)); // or decide a default/null object strategy

    var key = descriptorGuid.Value;

    if (cache.TryGetValue(key, out var cached))
        return cached;

    var descriptor = await dbContext.Descriptors
        .FirstOrDefaultAsync(d => d.GUID == key, ct)
        .ConfigureAwait(false);

    if (descriptor is null)
    {
        descriptor = new DescriptorDTO { GUID = key, Content = "none"};
        // Track it; DO NOT SaveChanges here
        await dbContext.Descriptors.AddAsync(descriptor, ct).ConfigureAwait(false);
    }

    cache[key] = descriptor;
    return descriptor;
}
}
