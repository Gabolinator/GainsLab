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
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.Sync;

/// <summary>
/// Applies descriptor sync payloads to the local SQLite database.
/// </summary>
public sealed class DescriptorSyncProcessor : ISyncEntityProcessor
{
    private const string SyncActor = "sync";

    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;

    public DescriptorSyncProcessor(IDbContextFactory<GainLabSQLDBContext> dbContextFactory, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    public EntityType EntityType => EntityType.Descriptor;

    public async Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct)
    {
        _logger?.Log(nameof(DescriptorSyncProcessor), $"Applying {items.Count} items");

        if (items.Count == 0) return Result.SuccessResult();

        var typed = items.OfType<DescriptorSyncDto>().ToList();
        if (typed.Count == 0) return Result.SuccessResult();

        _logger?.Log(nameof(DescriptorSyncProcessor), $"Applying {typed.Count} items of type {nameof(DescriptorSyncDto)}");

        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            foreach (var dto in typed)
            {
                
                ct.ThrowIfCancellationRequested();
                
                
                var entity = await dbContext.Descriptors
                    .FirstOrDefaultAsync(d => d.GUID == dto.GUID, ct)
                    .ConfigureAwait(false);

                if (entity is null)
                {
                    entity = new DescriptorDTO
                    {
                        GUID = dto.GUID,
                        CreatedAtUtc = dto.UpdatedAtUtc,
                        CreatedBy = SyncActor
                    };

                    await dbContext.Descriptors.AddAsync(entity, ct).ConfigureAwait(false);
                }

                entity.Content = dto.DescriptionContent;
                entity.UpdatedAtUtc = dto.UpdatedAtUtc;
                entity.UpdatedSeq = dto.UpdatedSeq;
                entity.UpdatedBy = SyncActor;
                entity.Version = dto.UpdatedSeq;
                entity.IsDeleted = dto.IsDeleted;
                entity.DeletedAt = dto.IsDeleted ? dto.UpdatedAtUtc : null;
                entity.DeletedBy = dto.IsDeleted ? SyncActor : null;
            }

            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.SuccessResult();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure("Sync cancelled");
        }
        catch (Exception ex)
        {
            _logger?.LogError(nameof(DescriptorSyncProcessor), $"Failed to apply descriptor sync: {ex.Message}");
            return Result.Failure($"Failed to apply descriptor sync: {ex.GetBaseException().Message}");
        }
    }
}
