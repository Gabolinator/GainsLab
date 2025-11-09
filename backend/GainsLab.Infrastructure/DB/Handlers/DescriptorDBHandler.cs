using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Database handler that manages descriptor DTO persistence inside the local SQLite database.
/// </summary>
public class DescriptorIdbHandler : IdbContextHandler<DescriptorDTO>
{
    public DescriptorIdbHandler(GainLabSQLDBContext context, ILogger logger) : base(context, logger)
    {
    }

    public override DbSet<DescriptorDTO> DBSet
        => ((GainLabSQLDBContext)_context).Descriptors;

    public override async Task<Result<DescriptorDTO>> TryGetExistingDTO(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            DescriptorDTO? existing = null;

            if (guid != Guid.Empty)
                existing = await query.FirstOrDefaultAsync(d => d.GUID == guid);

            if (existing is null && !string.IsNullOrWhiteSpace(content))
            {
                var normalized = NormalizeContent(content);
                existing = await query.FirstOrDefaultAsync(d =>
                    d.Content != null && d.Content.ToUpper() == normalized);
            }

            var success = existing != null;
            _logger.Log("DescriptorDbHandler",
                $"Existing descriptor lookup (guid: {guid}, content: {content ?? "<null>"}) -> {success}");

            return success
                ? Result<DescriptorDTO>.SuccessResult(existing!)
                : Result<DescriptorDTO>.Failure("No existing descriptor found");
        }
        catch (Exception ex)
        {
            _logger.LogError("DescriptorDbHandler", $"Exception in TryGetExistingDTO: {ex.Message}");
            return Result<DescriptorDTO>.Failure($"Error getting descriptor: {ex.GetBaseException().Message}");
        }
    }

    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var dtos = await DBSet
            .AsNoTracking()
            .ToListAsync(ct);

        var entities = dtos
            .Select(DescriptorMapper.ToDomain)
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

    private static string NormalizeContent(string value) =>
        value.Trim().ToUpperInvariant();
}
