
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.Results;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Database handler that manages descriptor Record persistence inside the local SQLite database.
/// </summary>
public class DescriptorIdbHandler : IdbContextHandler<DescriptorRecord>
{
    public DescriptorIdbHandler(GainLabSQLDBContext context, ILogger logger) : base(context, logger)
    {
    }

    public override DbSet<DescriptorRecord> DBSet
        => ((GainLabSQLDBContext)_context).Descriptors;

    public override async Task<Result<DescriptorRecord>> TryGetExistingRecord(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            DescriptorRecord? existing = null;

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
                ? Result<DescriptorRecord>.SuccessResult(existing!)
                : Result<DescriptorRecord>.Failure("No existing descriptor found");
        }
        catch (Exception ex)
        {
            _logger.LogError("DescriptorDbHandler", $"Exception in TryGetExistingRecord: {ex.Message}");
            return Result<DescriptorRecord>.Failure($"Error getting descriptor: {ex.GetBaseException().Message}");
        }
    }

    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var Records = await DBSet
            .AsNoTracking()
            .ToListAsync(ct);

        var entities = Records
            .Select(DescriptorMapper.ToDomain)
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

    private static string NormalizeContent(string value) =>
        value.Trim().ToUpperInvariant();
}
