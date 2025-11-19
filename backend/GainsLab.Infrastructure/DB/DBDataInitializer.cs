using System;
using System.Collections.Generic;
using System.Linq;
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
    private bool _verbose = true;

    /// <summary>
    /// Initializes a new instance of the <see cref="DBDataInitializer"/> class.
    /// </summary>
    /// <param name="logger">Logger used to record the seeding workflow.</param>
    /// <param name="clock">Clock used for timestamping seeded entities.</param>
    public DBDataInitializer(ILogger logger, IClock clock)
    {
        if(_verbose) _logger = logger;
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
        
        var added = new List<bool> {addedEquipment, addedMuscle,addedCategories };
        
        bool addedAny = added.Any(it=>it);

        if (addedAny)
        {
            if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Base Entities - Saving Changes");
            await db.SaveChangesAsync();
        }
        
    }

    // private async Task<bool> CreateBaseCategories(GainLabPgDBContext db, EntityFactory entityFactory)
    // {
    //     var existingNames = await db.MovementCategories
    //         .AsNoTracking()
    //         .Select(c => c.Name)
    //         .ToListAsync();
    //     var anyPresent = existingNames.Count > 0;
    //     if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Categories - Any Present:  {anyPresent}");
    //     
    //     if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Categories - Create Base Categories");
    //
    //     var categories = entityFactory.CreateBaseCategories();
    //     
    //     //for debug - comment me out after 
    //     foreach (var categoriesEntity in categories)
    //     {
    //         if(_verbose) _logger.Log(nameof(DBDataInitializer), "category: " +categoriesEntity.Content.ToString());
    //     }
    //
    //     var categoriesDtos = categories
    //         .Select(e => (MovementCategoryDTO)EntityDomainMapper.ToDTO(e)!)
    //         .ToList();
    //
    //     categoriesDtos = EnsureUniqueContent(
    //         categoriesDtos,
    //         dto => dto.Name,
    //         "movement categories",
    //         existingNames);
    //
    //     //for debug - comment me out after 
    //     foreach (var categoryDto in categoriesDtos)
    //     {
    //         if(_verbose) _logger.Log(nameof(DBDataInitializer),$" CategoryName - {categoryDto.Name.ToString()}");
    //     }
    //         
    //     var descriptions = categoriesDtos.Select(e => e.Descriptor).Where(d => d != null).Select(d=>d!);
    //
    //     //for debug - comment me out after 
    //     foreach (var description in descriptions)
    //     {
    //         if(_verbose) _logger.Log(description.ToString());
    //     }
    //         
    //     if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing categories - Adding {categoriesDtos.Count()} Base categories");
    //
    //     if (!categoriesDtos.Any())
    //     {
    //         if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing categories - No new categories to add.");
    //         return false;
    //     }
    //     
    //     db.MovementCategories.AddRange(categoriesDtos);
    //     
    //     var relationDtos = MovementCategoryMapper.CreateMovementCategoriesRelationDTOs(categoriesDtos, categories).ToList();
    //     if (relationDtos.Count > 0)
    //     {
    //         db.MovementCategoryRelations.AddRange(relationDtos);
    //     }
    //     
    //     return true;
    //
    //     
    // }
    private Task<bool> CreateBaseCategories(GainLabPgDBContext db, EntityFactory entityFactory)
    {
        return SeedBaseEntitiesAsync<MovementCategoryEntity, MovementCategoryDTO>(
            db,
            existingNameQuery: db.MovementCategories
                .AsNoTracking()
                .Select(c => c.Name),
            createBaseDomainEntities: entityFactory.CreateBaseCategories,
            mapToDto: domain => (MovementCategoryDTO)EntityDomainMapper.ToDTO(domain)!,
            nameSelector: dto => dto.Name,
            entityLabel: "movement categories",
            targetDbSet: db.MovementCategories,
            addExtraEntities: (ctx, categoryDtos, categoryDomainEntities) =>
            {
                var relationDtos = MovementCategoryMapper
                    .CreateMovementCategoriesRelationDTOs(categoryDtos, categoryDomainEntities)
                    .ToList();

                if (relationDtos.Count > 0)
                {
                    ctx.MovementCategoryRelations.AddRange(relationDtos);
                }
            });
    }
    
    
    private Task<bool> CreateBaseMuscles(GainLabPgDBContext db, EntityFactory entityFactory)
    { 
        return SeedBaseEntitiesAsync<MuscleEntity, MuscleDTO>(
            db,
            existingNameQuery: db.Muscles
                .AsNoTracking()
                .Select(m => m.Name),
            createBaseDomainEntities: entityFactory.CreateBaseMuscles,
            mapToDto: domain => (MuscleDTO)EntityDomainMapper.ToDTO(domain)!,
            nameSelector: dto => dto.Name,
            entityLabel: "muscles",
            targetDbSet: db.Muscles,
            addExtraEntities: (ctx, muscleDtos, muscleDomainEntities) =>
            {
                var antagonistDtos = MuscleMapper
                    .CreateMuscleAntagonistDTOs(muscleDtos, muscleDomainEntities)
                    .ToList();

                if (antagonistDtos.Count > 0)
                {
                    ctx.MuscleAntagonists.AddRange(antagonistDtos);
                }
            });

        
        // var existingNames = await db.Muscles
        //     .AsNoTracking()
        //     .Select(m => m.Name)
        //     .ToListAsync();
        // var anyPresent = existingNames.Count > 0;
        // if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Muscle - Any Present: { anyPresent}");
        //
        // if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Muscle - Create Base Muscle");
        //
        // var muscles = entityFactory.CreateBaseMuscles();
        //
        // //for debug - comment me out after 
        // foreach (var musclesEntity in muscles)
        // {
        //     if(_verbose) _logger.Log(nameof(DBDataInitializer), "musle: " +musclesEntity.ToString());
        // }
        //
        // var musclesDtos = muscles
        //     .Select(e => (MuscleDTO)EntityDomainMapper.ToDTO(e)!)
        //     .ToList();
        //
        // musclesDtos = EnsureUniqueContent(
        //     musclesDtos,
        //     dto => dto.Name,
        //     "muscles",
        //     existingNames);
        //
        // //for debug - comment me out after 
        // foreach (var muscleDto in musclesDtos)
        // {
        //     if(_verbose) _logger.Log(muscleDto.ToString());
        // }
        //     
        // var descriptions = musclesDtos.Select(e => e.Descriptor).Where(d => d != null).Select(d=>d!);
        //
        // //for debug - comment me out after 
        // foreach (var description in descriptions)
        // {
        //     if(_verbose) _logger.Log(description.ToString());
        // }
        //     
        // if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Muscles - Adding {musclesDtos.Count()} Base Muscles");
        //
        //      
        // //  db.Descriptors.AddRange(descriptions);
        // //  if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Descriptions - {descriptions.Count()} items");
        //
        //
        // if (!musclesDtos.Any())
        // {
        //     if(_verbose) _logger.Log(nameof(DBDataInitializer), "Initializing Muscle - No new muscles to add.");
        //     return false;
        // }
        //
        // db.Muscles.AddRange(musclesDtos);
        //
        // var antagonistDtos = MuscleMapper.CreateMuscleAntagonistDTOs(musclesDtos, muscles).ToList();
        // if (antagonistDtos.Count > 0)
        // {
        //     db.MuscleAntagonists.AddRange(antagonistDtos);
        // }
        //
        // return true;
        
    }

    /// <summary>
    /// Populates the database with the default set of equipment records when none exist.
    /// </summary>
    /// <param name="db">Database context used to check and persist equipment records.</param>
    /// <param name="entityFactory">Factory responsible for creating baseline equipment domain entities.</param>
    private Task<bool> CreateBaseEquipments(GainLabPgDBContext db, EntityFactory entityFactory)
    {
        return SeedBaseEntitiesAsync<EquipmentEntity, EquipmentDTO>(
            db,
            existingNameQuery: db.Equipments
                .AsNoTracking()
                .Select(e => e.Name),
            createBaseDomainEntities: entityFactory.CreateBaseEquipments,
            mapToDto: domain => (EquipmentDTO)domain.ToDTO()!,
            nameSelector: dto => dto.Name,
            entityLabel: "equipments",
            targetDbSet: db.Equipments
            // no extra entities like relations, so null
        );
        
        // if(_verbose) _logger.Log(nameof(DBDataInitializer),"Initializing Equipments");
        //
        // var existingNames = await db.Equipments
        //     .AsNoTracking()
        //     .Select(e => e.Name)
        //     .ToListAsync();
        // var anyPresent = existingNames.Count > 0;
        // if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Any Present: { anyPresent}");
        //
        // if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Create Base Equipments");
        //
        // var equipments = entityFactory.CreateBaseEquipments();
        //     foreach (var equipmentEntity in equipments)
        //     {
        //         if(_verbose) _logger.Log(equipmentEntity.ToString());
        //     }
        //
        //     var equipmentDtos = equipments
        //         .Select(e => (EquipmentDTO)e.ToDTO()!)
        //         .ToList();
        //
        //     equipmentDtos = EnsureUniqueContent(
        //         equipmentDtos,
        //         dto => dto.Name,
        //         "equipments",
        //         existingNames);
        //
        //     if (!equipmentDtos.Any())
        //     {
        //         if(_verbose) _logger.Log(nameof(DBDataInitializer), "Initializing Equipments - No new equipments to add.");
        //         return false;
        //     }
        //
        //     //for debug - comment me out after 
        //     foreach (var equipmentDto in equipmentDtos)
        //     {
        //         if(_verbose) _logger.Log(equipmentDto.ToString());
        //     }
        //     
        //     var descriptions = equipmentDtos.Select(e => e.Descriptor).Where(d => d != null).Select(d=>d!);
        //
        //     //for debug - comment me out after 
        //     foreach (var description in descriptions)
        //     {
        //         if(_verbose) _logger.Log(description.ToString());
        //     }
        //     
        //     if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Equipments - Adding {equipmentDtos.Count()} Base Equipments");
        //
        //     db.Equipments.AddRange(equipmentDtos);
        //  
        //     return true;
    }

    private List<TDto> EnsureUniqueContent<TDto>(
        IEnumerable<TDto> dtos,
        Func<TDto, string?> contentSelector,
        string entityLabel,
        IEnumerable<string>? existingEntries)
    {
        var filtered = new List<TDto>();
        var seenNew = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var existing = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        if (existingEntries is not null)
        {
            foreach (var entry in existingEntries)
            {
                if (string.IsNullOrWhiteSpace(entry)) continue;
                existing.Add(entry.Trim());
            }
        }

        foreach (var dto in dtos)
        {
            var content = contentSelector(dto);
            if (string.IsNullOrWhiteSpace(content))
                continue;

            var normalized = content.Trim();

            if (existing.Contains(normalized))
            {
                if(_verbose) _logger.Log(nameof(DBDataInitializer),
                    $"Skipping {entityLabel} '{normalized}' because it already exists.");
                continue;
            }

            if (!seenNew.Add(normalized))
            {
                if(_verbose) _logger.LogWarning(nameof(DBDataInitializer),
                    $"Duplicate {entityLabel} '{normalized}' detected in seed payload. Skipping duplicate.");
                continue;
            }

            filtered.Add(dto);
        }

        return filtered;
    }
    
    
    
    private async Task<bool> SeedBaseEntitiesAsync<TDomain, TDto>(
        GainLabPgDBContext db,
        IQueryable<string?> existingNameQuery,
        Func<IEnumerable<TDomain>> createBaseDomainEntities,
        Func<TDomain, TDto> mapToDto,
        Func<TDto, string?> nameSelector,
        string entityLabel,
        DbSet<TDto> targetDbSet,
        Action<GainLabPgDBContext, IReadOnlyList<TDto>, IReadOnlyList<TDomain>>? addExtraEntities = null)
        where TDomain : class
        where TDto : class
    {
        // 1. Load existing names
        var existingNames = await existingNameQuery
            .Where(n => n != null && n != "")
            .ToListAsync();

        var anyPresent = existingNames.Count > 0;
        if(_verbose) _logger.Log(nameof(DBDataInitializer),
            $"Initializing {entityLabel} - Any Present: {anyPresent}");

        // 2. Create domain entities
        if(_verbose) _logger.Log(nameof(DBDataInitializer),
            $"Initializing {entityLabel} - Create Base {entityLabel}");

        var domainEntities = createBaseDomainEntities().ToList();

        // 3. Map to DTOs
        var dtos = domainEntities
            .Select(mapToDto)
            .ToList();

        // 4. Filter out existing & duplicate names
        dtos = EnsureUniqueContent(
            dtos,
            nameSelector,
            entityLabel,
            existingNames);

        if (!dtos.Any())
        {
            if(_verbose) _logger.Log(nameof(DBDataInitializer),
                $"Initializing {entityLabel} - No new {entityLabel} to add.");
            return false;
        }

        if(_verbose) _logger.Log(nameof(DBDataInitializer),
            $"Initializing {entityLabel} - Adding {dtos.Count} base {entityLabel}");

        // 5. Add DTOs (+ extra entities like relations) with AutoDetectChanges disabled
        var oldAutoDetect = db.ChangeTracker.AutoDetectChangesEnabled;
        db.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            targetDbSet.AddRange(dtos);

            addExtraEntities?.Invoke(db, dtos, domainEntities);
        }
        finally
        {
            db.ChangeTracker.AutoDetectChangesEnabled = oldAutoDetect;
        }

        return true;
    }
    
    }

