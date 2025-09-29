
using GainsLab.Infrastructure.DB;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Handlers;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.DB.Model.Handlers;

public class EquipmentIdbHandler : IdbContextHandler<EquipmentDTO>
{
    public EquipmentIdbHandler(GainLabSQLDBContext context, ILogger logger) : base(context, logger)
    {
     
    }

    public override DbSet<EquipmentDTO> DBSet 
        => ((GainLabSQLDBContext)_context).Equipments;

    public override async Task<Result<EquipmentDTO>> TryGetExistingDTO(Guid guid)
    {
        try
        {
            var existing = await DBSet.FirstOrDefaultAsync(e => e.GUID == guid);
            var success = existing != null;

            _logger.Log("DbContextHandler",$" {guid} exist in db: {success}");

            return success ? Result<EquipmentDTO>.SuccessResult(existing!) : Result<EquipmentDTO>.Failure("No existing dto found");
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Exception in TryGetExistingDTO: {ex.Message}");
            throw;
        } 

    }
}