using System;
using System.Threading.Tasks;
using GainsLab.Models.Core.Results;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using GainsLab.Models.Factory;
using GainsLab.Models.Logging;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement.DB.Model.Handlers;

public class EquipmentIdbHandler : IdbContextHandler<EquipmentDTO>
{
    public EquipmentIdbHandler(GainLabDBContext context, IWorkoutLogger logger) : base(context, logger)
    {
     
    }

    public override DbSet<EquipmentDTO> DBSet 
        => ((GainLabDBContext)_context).Equipments;

    public override async Task<Result<EquipmentDTO>> TryGetExistingDTO(string uid)
    {
        try
        {
            var existing = await DBSet.FirstOrDefaultAsync(e => e.UID == uid);
            var success = existing != null;

            _logger.Log("DbContextHandler",$" {uid} exist in db: {success}");

            return success ? Results.SuccessResult(existing!) : Results.FailureResult<EquipmentDTO>("No existing dto found");
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Exception in TryGetExistingDTO: {ex.Message}");
            throw;
        } 

    }
}