using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Application.Results;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;
using GainsLab.Infrastructure.DB.Context;

using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

public class MovementCategoryIdbHandler: IdbContextHandler<MovementCategoryRecord>
{
    private readonly DescriptorIdbSQLHandler _descriptorHandler;
    
    public MovementCategoryIdbHandler(DbContext context, DescriptorIdbSQLHandler descriptorHandler ,ILogger logger) : base(context, logger)
    {
        _descriptorHandler = descriptorHandler;
        
    }

    public override DbSet<MovementCategoryRecord> DBSet =>  ((GainLabSQLDBContext)_context).MovementCategories;
    public override async Task<Result<MovementCategoryRecord>> TryGetExistingRecord(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            MovementCategoryRecord? existing = null;

            if (guid != Guid.Empty)
            {
                existing = await query.FirstOrDefaultAsync(e => e.GUID == guid);
            }

            string NormalizeContentStrategy(string value) =>
                value.Trim().ToUpperInvariant();

            if (existing is null && !string.IsNullOrWhiteSpace(content))
            {
                var normalized = NormalizeContent(content, NormalizeContentStrategy);
                existing = await query.FirstOrDefaultAsync(e =>
                    e.Name != null && e.Name.ToUpper() == normalized);
            }

            var success = existing != null;
            _logger.Log(nameof(MovementCategoryIdbHandler),
                $"Existing movement category lookup (guid: {guid}, content: {content ?? "<null>"}) -> {success}");

            return success
                ? Result<MovementCategoryRecord>.SuccessResult(existing!)
                : Result<MovementCategoryRecord>.Failure("No existing movement category found");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(MovementCategoryIdbHandler), $"Exception in TryGetExistingRecord: {ex.Message}");
            return Result<MovementCategoryRecord>.Failure($"Error getting movement category: {ex.GetBaseException().Message}");
        }

    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var Records = await DBSet
            .AsNoTracking()
            .Include(m => m.Descriptor)
            .Include(m => m.BaseCategoryLinks)
            .ThenInclude(link => link.ParentCategory)
            .ToListAsync(ct);

        var entities = Records
            .Select(MovementCategoryMapper.ToDomain)
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

    /// <summary>
    /// Ensures the descriptor reference is persisted and attached before saving the movement category.
    /// </summary>
    protected override async Task PrepareRelatedEntitiesAsync(MovementCategoryRecord Record, CancellationToken ct)
    {
        if (Record.Descriptor is null)
            return;

        if (Record.Descriptor.GUID == Guid.Empty && string.IsNullOrWhiteSpace(Record.Descriptor.Content))
        {
            _logger.LogWarning(nameof(MovementCategoryIdbHandler),
                $"Descriptor reference for movement category {Record.Iguid} is missing GUID/content. Skipping descriptor association.");
            Record.Descriptor = null;
            Record.DescriptorID = 0;
            return;
        }

        var descriptorResult = await _descriptorHandler.AddOrUpdateAsync(Record.Descriptor, save: false, ct)
            .ConfigureAwait(false);

        if (!descriptorResult.Success || descriptorResult.Value is not DescriptorRecord ensuredDescriptor)
        {
            var reason = descriptorResult.GetErrorMessage() ?? "Descriptor persistence failed";
            throw new InvalidOperationException($"Failed to ensure descriptor for movement category {Record.Iguid}: {reason}");
        }

        if (_context.Entry(ensuredDescriptor).State == EntityState.Detached)
            _context.Attach(ensuredDescriptor);

        Record.Descriptor = ensuredDescriptor;
        Record.DescriptorID = ensuredDescriptor.Iid;
    }

    
   
}
