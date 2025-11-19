using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DomainMappers;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

public class MovementCategoryIdbHandler: IdbContextHandler<MovementCategoryDTO>
{
    private readonly DescriptorIdbHandler _descriptorHandler;
    
    public MovementCategoryIdbHandler(DbContext context, DescriptorIdbHandler descriptorHandler ,ILogger logger) : base(context, logger)
    {
        _descriptorHandler = descriptorHandler;
        
    }

    public override DbSet<MovementCategoryDTO> DBSet =>  ((GainLabSQLDBContext)_context).MovementCategories;
    public override async Task<Result<MovementCategoryDTO>> TryGetExistingDTO(Guid guid, string? content)
    {
        try
        {
            var query = DBSet.AsNoTracking();
            MovementCategoryDTO? existing = null;

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
                ? Result<MovementCategoryDTO>.SuccessResult(existing!)
                : Result<MovementCategoryDTO>.Failure("No existing movement category found");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(MovementCategoryIdbHandler), $"Exception in TryGetExistingDTO: {ex.Message}");
            return Result<MovementCategoryDTO>.Failure($"Error getting movement category: {ex.GetBaseException().Message}");
        }

    }

    /// <inheritdoc />
    public override async Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default)
    {
        var dtos = await DBSet
            .AsNoTracking()
            .Include(m => m.Descriptor)
            .Include(m => m.BaseCategoryLinks)
            .ThenInclude(link => link.ParentCategory)
            .ToListAsync(ct);

        var entities = dtos
            .Select(MovementCategoryMapper.ToDomain)
            .Where(e => e is not null)
            .Cast<IEntity>()
            .ToList();

        return entities;
    }

    /// <summary>
    /// Ensures the descriptor reference is persisted and attached before saving the movement category.
    /// </summary>
    protected override async Task PrepareRelatedEntitiesAsync(MovementCategoryDTO dto, CancellationToken ct)
    {
        if (dto.Descriptor is null)
            return;

        if (dto.Descriptor.GUID == Guid.Empty && string.IsNullOrWhiteSpace(dto.Descriptor.Content))
        {
            _logger.LogWarning(nameof(MovementCategoryIdbHandler),
                $"Descriptor reference for movement category {dto.Iguid} is missing GUID/content. Skipping descriptor association.");
            dto.Descriptor = null;
            dto.DescriptorID = 0;
            return;
        }

        var descriptorResult = await _descriptorHandler.AddOrUpdateAsync(dto.Descriptor, save: false, ct)
            .ConfigureAwait(false);

        if (!descriptorResult.Success || descriptorResult.Value is not DescriptorDTO ensuredDescriptor)
        {
            var reason = descriptorResult.ErrorMessage ?? "Descriptor persistence failed";
            throw new InvalidOperationException($"Failed to ensure descriptor for movement category {dto.Iguid}: {reason}");
        }

        if (_context.Entry(ensuredDescriptor).State == EntityState.Detached)
            _context.Attach(ensuredDescriptor);

        dto.Descriptor = ensuredDescriptor;
        dto.DescriptorID = ensuredDescriptor.Iid;
    }

    
   
}
