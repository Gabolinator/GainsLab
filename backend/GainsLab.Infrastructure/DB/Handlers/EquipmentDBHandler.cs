using System;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.Results;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Database handler that manages equipment Record persistence inside the local SQLite database.
/// </summary>
public class EquipmentIdbHandler : IdbContextHandler<EquipmentRecord>
{
    private readonly DescriptorIdbSQLHandler _descriptorHandler;

    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentIdbHandler"/> class.
    /// </summary>
    /// <param name="context">EF Core context used for data access.</param>
    /// <param name="descriptorHandler"> For Inserting and validating descriptor Records</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    public EquipmentIdbHandler(GainLabSQLDBContext context, DescriptorIdbSQLHandler descriptorHandler ,ILogger logger) : base(context, logger)
    {
        _descriptorHandler = descriptorHandler;
    }

    /// <inheritdoc />
    public override DbSet<EquipmentRecord> DBSet 
        => ((GainLabSQLDBContext)_context).Equipments;

    /// <inheritdoc />
    public override async Task<Result<EquipmentRecord>> TryGetExistingRecord(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            EquipmentRecord? existing = null;

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
                $"Existing Record lookup (guid: {guid}, content: {content ?? "<null>"}) -> {success}");

            return success
                ? Result<EquipmentRecord>.SuccessResult(existing!)
                : Result<EquipmentRecord>.Failure("No existing Record found");
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Exception in TryGetExistingRecord: {ex.Message}");
            return Result<EquipmentRecord>.Failure($"Error getting Record: {ex.GetBaseException().Message}");
        }
    }

    private static string NormalizeContent(string value) =>
        value.Trim().ToUpperInvariant();

    protected override async Task PrepareRelatedEntitiesAsync(EquipmentRecord Record, CancellationToken ct)
    {
        if (Record.Descriptor is null)
            return;

        if (Record.Descriptor.GUID == Guid.Empty && string.IsNullOrWhiteSpace(Record.Descriptor.Content))
        {
            _logger.LogWarning(nameof(EquipmentIdbHandler),
                $"Descriptor reference for equipment {Record.Iguid} is missing GUID/content. Skipping descriptor association.");
            Record.Descriptor = null;
            Record.DescriptorID = 0;
            return;
        }

        var descriptorResult = await _descriptorHandler.AddOrUpdateAsync(Record.Descriptor, save: false, ct)
            .ConfigureAwait(false);

        if (!descriptorResult.Success || descriptorResult.Value is not DescriptorRecord ensuredDescriptor)
        {
            var reason = descriptorResult.GetErrorMessage() ?? "Descriptor persistence failed";
            throw new InvalidOperationException($"Failed to ensure descriptor for equipment {Record.Iguid}: {reason}");
        }

        if (_context.Entry(ensuredDescriptor).State == EntityState.Detached)
            _context.Attach(ensuredDescriptor);

        Record.Descriptor = ensuredDescriptor;
        Record.DescriptorID = ensuredDescriptor.Iid;
    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var Records = await DBSet
            .AsNoTracking()
            .ToListAsync(ct); // single round-trip

        // Map in memory; no translation issues.
        var entities = Records
            .Select(d => EquipmentMapper.ToDomain(d)) // specific mapper
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

   
}
