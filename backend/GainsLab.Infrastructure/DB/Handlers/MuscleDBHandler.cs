using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Results;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Database handler that manages muscle Record persistence, descriptors, and antagonist retrieval.
/// </summary>
public class MuscleIdbHandler : IdbContextHandler<MuscleRecord>
{
    private readonly DescriptorIdbSQLHandler _descriptorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleIdbHandler"/> class.
    /// </summary>
    public MuscleIdbHandler(
        GainLabSQLDBContext context,
        DescriptorIdbSQLHandler descriptorHandler,
        ILogger logger) : base(context, logger)
    {
        _descriptorHandler = descriptorHandler;
    }

    /// <inheritdoc />
    public override DbSet<MuscleRecord> DBSet
        => ((GainLabSQLDBContext)_context).Muscles;

    /// <inheritdoc />
    public override async Task<Result<MuscleRecord>> TryGetExistingRecord(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            MuscleRecord? existing = null;

            if (guid != Guid.Empty)
                existing = await query.FirstOrDefaultAsync(e => e.GUID == guid);

            string NormalizeContentStrategy(string value) =>
                value.Trim().ToUpperInvariant();
            
            if (existing is null && !string.IsNullOrWhiteSpace(content))
            {
                var normalized = NormalizeContent(content, NormalizeContentStrategy);
                existing = await query.FirstOrDefaultAsync(e =>
                    e.Name != null && e.Name.ToUpper() == normalized);
            }

            var success = existing != null;
            _logger.Log(nameof(MuscleIdbHandler),
                $"Existing muscle lookup (guid: {guid}, content: {content ?? "<null>"}) -> {success}");

            return success
                ? Result<MuscleRecord>.SuccessResult(existing!)
                : Result<MuscleRecord>.Failure("No existing muscle found");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(MuscleIdbHandler), $"Exception in TryGetExistingRecord: {ex.Message}");
            return Result<MuscleRecord>.Failure($"Error getting muscle: {ex.GetBaseException().Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var Records = await DBSet
            .AsNoTracking()
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
                .ThenInclude(link => link.Antagonist)
            .ToListAsync(ct);

        var entities = Records
            .Select(MuscleMapper.ToDomain)
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

    /// <summary>
    /// Ensures the descriptor reference is persisted and attached before saving the muscle.
    /// </summary>
    protected override async Task PrepareRelatedEntitiesAsync(MuscleRecord Record, CancellationToken ct)
    {
        if (Record.Descriptor is null)
            return;

        if (Record.Descriptor.GUID == Guid.Empty && string.IsNullOrWhiteSpace(Record.Descriptor.Content))
        {
            _logger.LogWarning(nameof(MuscleIdbHandler),
                $"Descriptor reference for muscle {Record.Iguid} is missing GUID/content. Skipping descriptor association.");
            Record.Descriptor = null;
            Record.DescriptorID = 0;
            return;
        }

        var descriptorResult = await _descriptorHandler.AddOrUpdateAsync(Record.Descriptor, save: false, ct)
            .ConfigureAwait(false);

        if (!descriptorResult.Success || descriptorResult.Value is not DescriptorRecord ensuredDescriptor)
        {
            var reason = descriptorResult.ErrorMessage ?? "Descriptor persistence failed";
            throw new InvalidOperationException($"Failed to ensure descriptor for muscle {Record.Iguid}: {reason}");
        }

        if (_context.Entry(ensuredDescriptor).State == EntityState.Detached)
            _context.Attach(ensuredDescriptor);

        Record.Descriptor = ensuredDescriptor;
        Record.DescriptorID = ensuredDescriptor.Iid;
    }

  
}
