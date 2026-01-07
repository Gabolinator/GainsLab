using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Equipment;
using GainsLab.Application.DTOs.Movement;
using GainsLab.Application.DTOs.MovementCategory;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.EntityFactory;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.Handlers;
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
    public async Task CreateBaseEntities(GainLabPgDBContext db, IEntitySeedResolver resolver)
    {
        
        await WarmSeedResolverAsync(db, resolver);
        
        var descriptionService = new BaseDescriptorService(_clock);
        
        var entityFactory = new EntityFactory(_clock,_logger, descriptionService, resolver);
        var entitySeeder = new EntitySeeder(_logger,entityFactory);
        
        //start by seeding "base block" to be sure we have the foundation to create other objects 
        
        var addedEquipment = await CreateBaseEquipments(db, entitySeeder);
        var addedMuscle = await CreateBaseMuscles(db, entitySeeder);
        var addedCategories = await CreateBaseCategories(db, entitySeeder);
       
        
        bool prerequisitesAdded = new List<bool> {addedEquipment, addedMuscle,addedCategories}.Any(it=>it);

        if (prerequisitesAdded)
        {
            if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Base Entities - Saving Changes");
            await db.SaveChangesAsync();
            await WarmSeedResolverAsync(db, resolver);
            
            //resolve missing link here - example resolve the variantOf of a movement ? 
            
        }
        
        //we probably need to update the cache 
        
        //then add move complex objects 
        var addedMovements = await CreateBaseMovements(db, entitySeeder);

       bool addedMovementsAny = addedMovements;

        if (addedMovementsAny)
        {
            if(_verbose) _logger.Log(nameof(DBDataInitializer),$"Initializing Base Entities - Saving Changes");
            await db.SaveChangesAsync();
            await WarmSeedResolverAsync(db, resolver);
            
            //resolve missing link here - example resolve the variantOf of a movement ? 
            
        }
        
        
    }

    private Task<bool> CreateBaseMovements(GainLabPgDBContext db, EntitySeeder entitySeeder)
    {
        return SeedBaseEntitiesAsync<MovementEntity, MovementRecord>(
            db,
            existingNameQuery: db.MovementCategories
                .AsNoTracking()
                .Select(c => c.Name),
            createBaseDomainEntities: entitySeeder.CreateBaseMovements,
            mapToDto: domain => (MovementRecord)EntityDomainMapper.ToRecord(domain, _clock)!,
            nameSelector: dto => dto.Name,
            entityLabel: "movement categories",
            targetDbSet: db.Movement);
        
    }
    
    private Task<bool> CreateBaseCategories(GainLabPgDBContext db, EntitySeeder entitySeeder)
    {
        return SeedBaseEntitiesAsync<MovementCategoryEntity, MovementCategoryRecord>(
            db,
            existingNameQuery: db.MovementCategories
                .AsNoTracking()
                .Select(c => c.Name),
            createBaseDomainEntities: entitySeeder.CreateBaseCategories,
            mapToDto: domain => (MovementCategoryRecord)EntityDomainMapper.ToRecord(domain, _clock)!,
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
    
    
    private Task<bool> CreateBaseMuscles(GainLabPgDBContext db, EntitySeeder entitySeeder)
    { 
        return SeedBaseEntitiesAsync<MuscleEntity, MuscleRecord>(
            db,
            existingNameQuery: db.Muscles
                .AsNoTracking()
                .Select(m => m.Name),
            createBaseDomainEntities: entitySeeder.CreateBaseMuscles,
            mapToDto: domain => (MuscleRecord)EntityDomainMapper.ToRecord(domain, _clock)!,
            nameSelector: dto => dto.Name,
            entityLabel: "muscles",
            targetDbSet: db.Muscles,
            addExtraEntities: (ctx, muscleDtos, muscleDomainEntities) =>
            {
                var antagonistDtos = MuscleMapper
                    .CreateMuscleAntagonistRecords(muscleDtos, muscleDomainEntities)
                    .ToList();

                if (antagonistDtos.Count > 0)
                {
                    ctx.MuscleAntagonists.AddRange(antagonistDtos);
                }
            });
    }

    private async Task WarmSeedResolverAsync(GainLabPgDBContext db, IEntitySeedResolver resolver)
    {
        await TrackExistingEquipmentsAsync(db, resolver);
        await TrackExistingMusclesAsync(db, resolver);
        await TrackExistingCategoriesAsync(db, resolver);
    }

    private static async Task TrackExistingEquipmentsAsync(GainLabPgDBContext db, IEntitySeedResolver resolver)
    {
        resolver.Clear<EquipmentEntity>();
        var equipments = await db.Equipments
            .AsNoTracking()
            .Include(e => e.Descriptor)
            .ToListAsync();

        foreach (var dto in equipments)
        {
            if (EntityDomainMapper.ToDomain(dto) is EquipmentEntity equipment &&
                !string.IsNullOrWhiteSpace(equipment.Content.Name))
            {
                resolver.Track(equipment.Content.Name, equipment);
            }
        }
    }

    private static async Task TrackExistingMusclesAsync(GainLabPgDBContext db, IEntitySeedResolver resolver)
    {
        resolver.Clear<MuscleEntity>();
        var muscles = await db.Muscles
            .AsNoTracking()
            .Include(m => m.Descriptor)
            .Include(m => m.Antagonists)
                .ThenInclude(link => link.Antagonist)
            .ToListAsync();

        foreach (var dto in muscles)
        {
            if (EntityDomainMapper.ToDomain(dto) is MuscleEntity muscle &&
                !string.IsNullOrWhiteSpace(muscle.Content.Name))
            {
                resolver.Track(muscle.Content.Name, muscle);
            }
        }
    }

    private static async Task TrackExistingCategoriesAsync(GainLabPgDBContext db, IEntitySeedResolver resolver)
    {
        resolver.Clear<MovementCategoryEntity>();
        var categories = await db.MovementCategories
            .AsNoTracking()
            .Include(c => c.Descriptor)
            .Include(c => c.ParentCategory)
            .Include(c => c.BaseCategoryLinks)
                .ThenInclude(link => link.ParentCategory)
            .ToListAsync();

        foreach (var dto in categories)
        {
            if (EntityDomainMapper.ToDomain(dto) is MovementCategoryEntity category &&
                !string.IsNullOrWhiteSpace(category.Content.Name))
            {
                resolver.Track(category.Content.Name, category);
            }
        }
    }
    

    /// <summary>
    /// Populates the database with the default set of equipment records when none exist.
    /// </summary>
    /// <param name="db">Database context used to check and persist equipment records.</param>
    /// <param name="entityFactory">Factory responsible for creating baseline equipment domain entities.</param>
    private Task<bool> CreateBaseEquipments(GainLabPgDBContext db, EntitySeeder entitySeeder)
    {
        return SeedBaseEntitiesAsync<EquipmentEntity, EquipmentRecord>(
            db,
            existingNameQuery: db.Equipments
                .AsNoTracking()
                .Select(e => e.Name),
            createBaseDomainEntities: entitySeeder.CreateBaseEquipments,
            mapToDto: domain => (EquipmentRecord)domain.ToRecord(_clock)!,
            nameSelector: dto => dto.Name,
            entityLabel: "equipments",
            targetDbSet: db.Equipments
            // no extra entities like relations, so null
        );
        
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

