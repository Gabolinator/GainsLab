using GainsLab.Core.Models.Core.Factory;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Infrastructure.DB.Handlers;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB;

/// <summary>
/// Seeds required baseline data into the GainsLab database so the application can run with defaults.
/// </summary>
public class DBDataInitializer
{
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBDataInitializer"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record the seeding workflow.</param>
    public DBDataInitializer(ILogger logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Ensures that core reference entities exist in the database.
    /// </summary>
    /// <param name="db">The Postgres context used for querying and persisting entities.</param>
    public async Task CreateBaseEntities(GainLabPgDBContext db)
    {

        
        var descriptionService = new BaseDescriptorService(CoreUtilities.Clock);
        
        var entityFactory = new EntityFactory(CoreUtilities.Clock,_logger, descriptionService);
        
        _logger.Log(nameof(DBDataInitializer),"Initializing Equipments");
        await CreateBaseEquipments(db, entityFactory);
        
        await db.SaveChangesAsync();
        
    }

    /// <summary>
    /// Populates the database with the default set of equipment records when none exist.
    /// </summary>
    /// <param name="db">Database context used to check and persist equipment records.</param>
    /// <param name="entityFactory">Factory responsible for creating baseline equipment domain entities.</param>
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
            
            var descriptions = equipmentDtos.Select(e => e.Descriptor).Where(d => d != null).Select(d=>d!);

            foreach (var description in descriptions)
            {
                _logger.Log(description.ToString());
            }
            
            _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Adding {equipmentDtos.Count()} Base Equipments");

             
            db.Descriptors.AddRange(descriptions);
            _logger.Log(nameof(DBDataInitializer),$"Initializing Descriptions - {descriptions.Count()} items");

            db.Equipments.AddRange(equipmentDtos);
            _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Saving Equipments Changes");

           
        }
    }

