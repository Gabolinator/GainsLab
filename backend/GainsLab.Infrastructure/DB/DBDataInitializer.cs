using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Factory;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DomainMappers;
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
    private readonly IClock _clock;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBDataInitializer"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record the seeding workflow.</param>
    /// <param name="clock">Clock used for timestamping seeded entities.</param>
    public DBDataInitializer(ILogger logger, IClock clock)
    {
        _logger = logger;
        _clock = clock;
    }

    /// <summary>
    /// Ensures that core reference entities exist in the database.
    /// </summary>
    /// <param name="db">The Postgres context used for querying and persisting entities.</param>
    public async Task CreateBaseEntities(GainLabPgDBContext db)
    {
        
        var descriptionService = new BaseDescriptorService(_clock);
        
        var entityFactory = new EntityFactory(_clock,_logger, descriptionService);
        
        
        var addedEquipment = await CreateBaseEquipments(db, entityFactory);
        var addedMuscle = await CreateBaseMuscles(db, entityFactory);
        var addedCategories = await CreateBaseCategories(db, entityFactory);
        
        bool addedAny = addedEquipment || addedMuscle;

        if (addedAny)
        {
            _logger.Log(nameof(DBDataInitializer),$"Initializing Base Entities - Saving Changes");
            await db.SaveChangesAsync();
        }
        
    }

    private async Task<bool> CreateBaseCategories(GainLabPgDBContext db, EntityFactory entityFactory)
    {
        var anyPresent = await db.MovementCategories.AnyAsync();
        _logger.Log(nameof(DBDataInitializer),$"Initializing Categories - Any Present:  {anyPresent}");
        
        if (anyPresent)
        {
            _logger.Log(nameof(DBDataInitializer),$"Initializing Categories - Dont add base Categories - already present");
            return false; 
        }
        
        _logger.Log(nameof(DBDataInitializer),$"Initializing Categories - Create Base Categories");

        var categories = entityFactory.CreateBaseCategories();
        
        foreach (var categoriesEntity in categories)
        {
            _logger.Log(nameof(DBDataInitializer), "category: " +categoriesEntity.ToString());
        }

        var categoriesDtos = categories
            .Select(e => (MovementCategoryDTO)EntityDomainMapper.ToDTO(e)!)
            .ToList();

        foreach (var categoryDto in categoriesDtos)
        {
            _logger.Log(categoryDto.ToString());
        }
            
        var descriptions = categoriesDtos.Select(e => e.Descriptor).Where(d => d != null).Select(d=>d!);

        foreach (var description in descriptions)
        {
            _logger.Log(description.ToString());
        }
            
        _logger.Log(nameof(DBDataInitializer),$"Initializing categories - Adding {categoriesDtos.Count()} Base categories");

             
        //  db.Descriptors.AddRange(descriptions);
        //  _logger.Log(nameof(DBDataInitializer),$"Initializing Descriptions - {descriptions.Count()} items");

        
        //nothing to add
        if (!categoriesDtos.Any()) return false;
        
        db.MovementCategories.AddRange(categoriesDtos);
        
        var relationDtos = MovementCategoryMapper.CreateMovementCategoriesRelationDTOs(categoriesDtos, categories).ToList();
        if (relationDtos.Count > 0)
        {
            db.MovementCategoryRelations.AddRange(relationDtos);
        }
        
        return true;

        
    }

    private async Task<bool> CreateBaseMuscles(GainLabPgDBContext db, EntityFactory entityFactory)
    { 
        var anyPresent = await db.Muscles.AnyAsync();
        _logger.Log(nameof(DBDataInitializer),$"Initializing Muscle - Any Present: { anyPresent}");

        if (anyPresent)
        {
            _logger.Log(nameof(DBDataInitializer),$"Initializing Muscle - Dont add base Muscle - already present");
            return false; 
        }
        
        _logger.Log(nameof(DBDataInitializer),$"Initializing Muscle - Create Base Muscle");

        var muscles = entityFactory.CreateBaseMuscles();
        foreach (var musclesEntity in muscles)
        {
            _logger.Log(nameof(DBDataInitializer), "musle: " +musclesEntity.ToString());
        }

        var musclesDtos = muscles
            .Select(e => (MuscleDTO)EntityDomainMapper.ToDTO(e)!)
            .ToList();

        foreach (var muscleDto in musclesDtos)
        {
            _logger.Log(muscleDto.ToString());
        }
            
        var descriptions = musclesDtos.Select(e => e.Descriptor).Where(d => d != null).Select(d=>d!);

        foreach (var description in descriptions)
        {
            _logger.Log(description.ToString());
        }
            
        _logger.Log(nameof(DBDataInitializer),$"Initializing Muscles - Adding {musclesDtos.Count()} Base Muscles");

             
        //  db.Descriptors.AddRange(descriptions);
        //  _logger.Log(nameof(DBDataInitializer),$"Initializing Descriptions - {descriptions.Count()} items");

        
        //nothing to add
        if (!musclesDtos.Any()) return false;
        
        db.Muscles.AddRange(musclesDtos);
        
        var antagonistDtos = MuscleMapper.CreateMuscleAntagonistDTOs(musclesDtos, muscles).ToList();
        if (antagonistDtos.Count > 0)
        {
            db.MuscleAntagonists.AddRange(antagonistDtos);
        }
        
        return true;
        
    }

    /// <summary>
    /// Populates the database with the default set of equipment records when none exist.
    /// </summary>
    /// <param name="db">Database context used to check and persist equipment records.</param>
    /// <param name="entityFactory">Factory responsible for creating baseline equipment domain entities.</param>
    private async Task<bool> CreateBaseEquipments(GainLabPgDBContext db, EntityFactory entityFactory)
    {
        _logger.Log(nameof(DBDataInitializer),"Initializing Equipments");
        
        var anyPresent = await db.Equipments.AnyAsync();
        _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Any Present: { anyPresent}");

        if (anyPresent)
        {
            _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Dont add base equipments");
           return false; 
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

             
          //  db.Descriptors.AddRange(descriptions);
          //  _logger.Log(nameof(DBDataInitializer),$"Initializing Descriptions - {descriptions.Count()} items");

            db.Equipments.AddRange(equipmentDtos);
         
            return true;
    }
    }

