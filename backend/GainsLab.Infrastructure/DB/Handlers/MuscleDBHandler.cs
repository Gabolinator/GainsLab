using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DomainMappers;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Database handler that manages muscle DTO persistence, descriptors, and antagonist retrieval.
/// </summary>
public class MuscleIdbHandler : IdbContextHandler<MuscleDTO>
{
    private readonly DescriptorIdbHandler _descriptorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="MuscleIdbHandler"/> class.
    /// </summary>
    public MuscleIdbHandler(
        GainLabSQLDBContext context,
        DescriptorIdbHandler descriptorHandler,
        ILogger logger) : base(context, logger)
    {
        _descriptorHandler = descriptorHandler;
    }

    /// <inheritdoc />
    public override DbSet<MuscleDTO> DBSet
        => ((GainLabSQLDBContext)_context).Muscles;

    /// <inheritdoc />
    public override async Task<Result<MuscleDTO>> TryGetExistingDTO(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            MuscleDTO? existing = null;

            if (guid != Guid.Empty)
                existing = await query.FirstOrDefaultAsync(e => e.GUID == guid);

            if (existing is null && !string.IsNullOrWhiteSpace(content))
            {
                var normalized = NormalizeContent(content);
                existing = await query.FirstOrDefaultAsync(e =>
                    e.Name != null && e.Name.ToUpper() == normalized);
            }

            var success = existing != null;
            _logger.Log(nameof(MuscleIdbHandler),
                $"Existing muscle lookup (guid: {guid}, content: {content ?? "<null>"}) -> {success}");

            return success
                ? Result<MuscleDTO>.SuccessResult(existing!)
                : Result<MuscleDTO>.Failure("No existing muscle found");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(MuscleIdbHandler), $"Exception in TryGetExistingDTO: {ex.Message}");
            return Result<MuscleDTO>.Failure($"Error getting muscle: {ex.GetBaseException().Message}");
        }
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var dtos = await DBSet
            .AsNoTracking()
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
                .ThenInclude(link => link.Antagonist)
            .ToListAsync(ct);

        var entities = dtos
            .Select(MuscleMapper.ToDomain)
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

    /// <summary>
    /// Ensures the descriptor reference is persisted and attached before saving the muscle.
    /// </summary>
    protected override async Task PrepareRelatedEntitiesAsync(MuscleDTO dto, CancellationToken ct)
    {
        if (dto.Descriptor is null)
            return;

        if (dto.Descriptor.GUID == Guid.Empty && string.IsNullOrWhiteSpace(dto.Descriptor.Content))
        {
            _logger.LogWarning(nameof(MuscleIdbHandler),
                $"Descriptor reference for muscle {dto.Iguid} is missing GUID/content. Skipping descriptor association.");
            dto.Descriptor = null;
            dto.DescriptorID = 0;
            return;
        }

        var descriptorResult = await _descriptorHandler.AddOrUpdateAsync(dto.Descriptor, save: false, ct)
            .ConfigureAwait(false);

        if (!descriptorResult.Success || descriptorResult.Value is not DescriptorDTO ensuredDescriptor)
        {
            var reason = descriptorResult.ErrorMessage ?? "Descriptor persistence failed";
            throw new InvalidOperationException($"Failed to ensure descriptor for muscle {dto.Iguid}: {reason}");
        }

        if (_context.Entry(ensuredDescriptor).State == EntityState.Detached)
            _context.Attach(ensuredDescriptor);

        dto.Descriptor = ensuredDescriptor;
        dto.DescriptorID = ensuredDescriptor.Iid;
    }

    private static string NormalizeContent(string value) =>
        value.Trim().ToUpperInvariant();
}
