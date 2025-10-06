using GainsLab.Core.Models.Core.Factory;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Handlers;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using GainsLab.Models.Logging;
using GainsLab.Models.Utilities;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB;

public class DBDataInitializer
{
    private readonly ILogger _logger;

    public DBDataInitializer(ILogger logger)
    {
        _logger = logger;
    }

    public async Task CreateBaseEntities(GainLabPgDBContext db)
    {

        
        var descriptionService = new BaseDescriptorService(CoreUtilities.Clock);
        
        var entityFactory = new EntityFactory(CoreUtilities.Clock,_logger, descriptionService);
        
        _logger.Log(nameof(DBDataInitializer),"Initializing Equipments");
        await CreateBaseEquipments(db, entityFactory);
        
    }

    private async Task CreateBaseEquipments(GainLabPgDBContext db, EntityFactory entityFactory)
    {
        var anyPresent = await db.Equipments.AnyAsync();
        _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Any Present: { anyPresent}");

        if (anyPresent)
        {
            _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Dont add base equipments");
           return; 
        }
        
        _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Create Base Equipments");

        var equipments = entityFactory.CreateBaseEquipments();
            foreach (var equipmentEntity in equipments)
            {
                _logger.Log(equipmentEntity.ToString());
            }

            var equipmentDtos = equipments.Select(e => (EquipmentDTO)e.ToDTO()!);

            foreach (var equipmentDto in equipmentDtos)
            {
                _logger.Log(equipmentDto.ToString());
            }

            _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Adding {equipmentDtos.Count()} Base Equipments");

              db.Equipments.AddRange(equipmentDtos);
              _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Saving Equipments Changes");

              await db.SaveChangesAsync();
        }
    }

