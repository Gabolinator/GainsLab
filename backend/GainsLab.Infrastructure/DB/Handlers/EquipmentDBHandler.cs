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
    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentIdbHandler"/> class.
    /// </summary>
    /// <param name="context">EF Core context used for data access.</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    public EquipmentIdbHandler(GainLabSQLDBContext context, ILogger logger) : base(context, logger)
    {
     
    }

    /// <inheritdoc />
    public override DbSet<EquipmentDTO> DBSet 
        => ((GainLabSQLDBContext)_context).Equipments;

    /// <inheritdoc />
    public override async Task<Result<EquipmentDTO>> TryGetExistingDTO(Guid guid)
    {
        try
        {
            var existing = await DBSet
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.GUID == guid);

            var success = existing != null;
            _logger.Log("DbContextHandler", $"{guid} exists in db: {success}");

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
