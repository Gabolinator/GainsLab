using GainsLab.Application.DTOs;
using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Results;
using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.Sync.Processor;

/// <summary>
/// Applies equipment sync payloads to the local SQLite database.
/// </summary>
public sealed class EquipmentSyncProcessor : ISyncEntityProcessor
{
    private const string SyncActor = "sync";

    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;
    private readonly IDescriptorResolver _descriptorResolver;

    /// <summary>
    /// Creates a processor that materializes remote equipment payloads into SQLite entities.
    /// </summary>
    public EquipmentSyncProcessor(IDbContextFactory<GainLabSQLDBContext> dbContextFactory,IDescriptorResolver descriptorResolver, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
        _descriptorResolver = descriptorResolver;
    }

    /// <inheritdoc />
    public EntityType EntityType => EntityType.Equipment;

    /// <inheritdoc />
public async Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct)
{
    _logger?.Log(nameof(EquipmentSyncProcessor), $"Applying Async for {items.Count} items");

    
    if (items.Count == 0) return Result.SuccessResult();

    var typed = items.OfType<EquipmentSyncDTO>().ToList();
    if (typed.Count == 0) return Result.SuccessResult();

    try
    {
        
        _logger?.Log(nameof(EquipmentSyncProcessor), $"Applying Async for {items.Count} {nameof(EquipmentSyncDTO)}");

        await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);
        var descriptorCache = new Dictionary<Guid, DescriptorRecord>();

        
        foreach (var dto in typed)
        {
            ct.ThrowIfCancellationRequested();

            _logger?.Log(nameof(EquipmentSyncProcessor), $"Applying Async for {nameof(EquipmentSyncDTO)} : {dto.Name} | {dto.GUID} | {(dto.DescriptorGUID == null? "null descriptor guid" :dto.DescriptorGUID) }");

            
            // Resolve without saving here
            var descriptor = await _descriptorResolver.ResolveDescriptorAsync(dbContext, dto.DescriptorGUID, descriptorCache, ct)
                .ConfigureAwait(false);

            var entity = await dbContext.Equipments
                .Include(e => e.Descriptor)
                .FirstOrDefaultAsync(e => e.GUID == dto.GUID, ct)
                .ConfigureAwait(false);

            if (entity is null)
            {
                entity = new EquipmentRecord
                {
                    Name = dto.Name,
                    GUID = dto.GUID,
                    CreatedAtUtc = dto.UpdatedAtUtc,
                    CreatedBy = SyncActor,
                    Authority = dto.Authority
                };

                await dbContext.Equipments.AddAsync(entity, ct).ConfigureAwait(false);
            }

            
            
            entity.Name = dto.Name;

            // Set the relationship; EF will handle DescriptorID
            entity.Descriptor = descriptor;
            entity.Authority = dto.Authority;

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
}
