using System;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DomainMappers;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Database handler that manages equipment DTO persistence inside the local SQLite database.
/// </summary>
public class EquipmentIdbHandler : IdbContextHandler<EquipmentDTO>
{
    private readonly DescriptorIdbHandler _descriptorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentIdbHandler"/> class.
    /// </summary>
    /// <param name="context">EF Core context used for data access.</param>
    /// <param name="descriptorHandler"> For Inserting and validating descriptor dtos</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    public EquipmentIdbHandler(GainLabSQLDBContext context, DescriptorIdbHandler descriptorHandler ,ILogger logger) : base(context, logger)
    {
        _descriptorHandler = descriptorHandler;
    }

    /// <inheritdoc />
    public override DbSet<EquipmentDTO> DBSet 
        => ((GainLabSQLDBContext)_context).Equipments;

    /// <inheritdoc />
    public override async Task<Result<EquipmentDTO>> TryGetExistingDTO(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            EquipmentDTO? existing = null;

            if (guid != Guid.Empty)
                existing = await query.FirstOrDefaultAsync(e => e.GUID == guid);

            if (existing is null && !string.IsNullOrWhiteSpace(content))
            {
                var normalized = NormalizeContent(content);
                existing = await query.FirstOrDefaultAsync(e =>
                    e.Name != null && e.Name.ToUpper() == normalized);
            }

            var success = existing != null;
            _logger.Log("DbContextHandler",
                $"Existing DTO lookup (guid: {guid}, content: {content ?? "<null>"}) -> {success}");

            return success
                ? Result<EquipmentDTO>.SuccessResult(existing!)
                : Result<EquipmentDTO>.Failure("No existing dto found");
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Exception in TryGetExistingDTO: {ex.Message}");
            return Result<EquipmentDTO>.Failure($"Error getting DTO: {ex.GetBaseException().Message}");
        }
    }

    private static string NormalizeContent(string value) =>
        value.Trim().ToUpperInvariant();

    protected override async Task PrepareRelatedEntitiesAsync(EquipmentDTO dto, CancellationToken ct)
    {
        if (dto.Descriptor is null)
            return;

        if (dto.Descriptor.GUID == Guid.Empty && string.IsNullOrWhiteSpace(dto.Descriptor.Content))
        {
            _logger.LogWarning(nameof(EquipmentIdbHandler),
                $"Descriptor reference for equipment {dto.Iguid} is missing GUID/content. Skipping descriptor association.");
            dto.Descriptor = null;
            dto.DescriptorID = 0;
            return;
        }

        var descriptorResult = await _descriptorHandler.AddOrUpdateAsync(dto.Descriptor, save: false, ct)
            .ConfigureAwait(false);

        if (!descriptorResult.Success || descriptorResult.Value is not DescriptorDTO ensuredDescriptor)
        {
            var reason = descriptorResult.ErrorMessage ?? "Descriptor persistence failed";
            throw new InvalidOperationException($"Failed to ensure descriptor for equipment {dto.Iguid}: {reason}");
        }

        if (_context.Entry(ensuredDescriptor).State == EntityState.Detached)
            _context.Attach(ensuredDescriptor);

        dto.Descriptor = ensuredDescriptor;
        dto.DescriptorID = ensuredDescriptor.Iid;
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var dtos = await DBSet
            .AsNoTracking()
            .ToListAsync(ct); // single round-trip

        // Map in memory; no translation issues.
        var entities = dtos
            .Select(d => EquipmentMapper.ToDomain(d)) // specific mapper
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

   
}
