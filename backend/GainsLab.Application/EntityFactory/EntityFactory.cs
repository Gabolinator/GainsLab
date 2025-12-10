using GainsLab.Application.Interfaces;
using GainsLab.Domain;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.EntityFactory;

/// <summary>
/// Coordinates creation of core domain entities and their descriptors.
/// </summary>
public class EntityFactory
{

    public EntityFactory(IClock clock, ILogger logger ,IDescriptorService<BaseDescriptorEntity> descriptorService, IEntitySeedResolver resolver)
    {
        _logger = logger;
        _clock = clock;
        _equipmentFactory = new EquipmentFactory(clock, descriptorService, resolver );
        _descriptorFactory = new DescriptorFactory(clock, descriptorService , resolver);
        _muscleFactory = new MuscleFactory(clock, descriptorService, resolver);
        _movementCategoryFactory = new MovementCategoryFactory(clock, descriptorService, resolver);
        _movementFactory = new MovementFactory(clock, descriptorService, resolver);
        _resolver = resolver;
    }

    private readonly ILogger _logger;
    private readonly EquipmentFactory _equipmentFactory;
    private readonly DescriptorFactory _descriptorFactory;
    private readonly MuscleFactory _muscleFactory;
    private readonly MovementCategoryFactory _movementCategoryFactory;
    private readonly IClock _clock;
    private readonly MovementFactory _movementFactory;
    private readonly IEntitySeedResolver _resolver;





    public AuditedInfo GetDefaultAudit(string creator = "system")
    {
       return AuditedInfo.New(_clock.UtcNow, creator);
    }

    public EquipmentEntity GetOrCreateEquipment(string name, string description, string creator = "system")
    {
       // var auditInfo = GetDefaultAudit(creator);
      //  var descriptor = CreateBaseDescriptor(description, auditInfo);
        
        
        //
        // return _equipmentFactory.GetOrCreate(new EquipmentCreationConfig
        // {
        //     Id = EquipmentId.New(),
        //     Content = new EquipmentContent(name),
        //     Audit = auditInfo,
        //     Descriptor = CreateBaseDescriptor(description, auditInfo)
        // });

        return GetOrCreateEntity(name, 
            () => CreateEquipment(name,description,creator));
    }

    public MuscleEntity GetOrCreateMuscle(
        string name,
        string latinName,
        string description,
        eBodySection bodySection,
        string creator = "system",
        BaseDescriptorEntity? descriptor = null,
        params MuscleId[] antagonists)
    {
        return GetOrCreateEntity(name, () =>
            CreateMuscle(name, latinName, description, bodySection, creator, descriptor, antagonists));
    }

    public MovementCategoryEntity GetOrCreateMovementCategory(
        string name,
        string description,
        string creator = "system",
        BaseDescriptorEntity? descriptor = null,
        MovementCategoryId? parentId = null,
        params eMovementCategories[] baseCategories)
    {
        return GetOrCreateEntity(name, () =>
            CreateMovementCategory(name, description, creator, descriptor, parentId, baseCategories));
    }

    public EquipmentEntity CreateEquipment(string name, string description ,string creator = "system")
    {
        var auditInfo = GetDefaultAudit(creator);

      
        
        var eq = _equipmentFactory.Create(new EquipmentCreationConfig
        {
            Id = EquipmentId.New(),
            Content = new EquipmentContent(name),
            Audit = auditInfo,
            Descriptor = CreateBaseDescriptor(description, auditInfo)
        });

        
        return eq;
    }

   
    public BaseDescriptorEntity CreateBaseDescriptor(string description ,AuditedInfo? auditedInfo = null)
    {
        var auditInfo = auditedInfo ?? GetDefaultAudit();
        
        return _descriptorFactory.Create(new DescriptiorCreationConfig
        {
            Id = DescriptorId.New(),
            Content = new BaseDescriptorContent
            {
                Description = Description.New(description)
            },
            Audit = auditInfo
        });
    }

    public MuscleEntity CreateMuscle(
        string name,
        string latinName,
        string description,
        eBodySection bodySection,
        string creator = "system",
        BaseDescriptorEntity? descriptor = null,
        params MuscleId[] antagonists)
    {
        var auditInfo = GetDefaultAudit(creator);

        descriptor ??= CreateBaseDescriptor(description, auditInfo);

        var muscleContent = new MuscleContent
        {
            Name = name,
            LatinName = latinName,
            BodySection = bodySection
        };

        _logger.Log(nameof(EntityFactory), $"Creating Muscle : {muscleContent.Name}" );

        return _muscleFactory.Create(new MuscleCreationConfig
        {
            Content = muscleContent,
            Audit = auditInfo,
            Descriptor = descriptor,
            CreatedBy = creator,
            Antagonists = antagonists
        });
        
    }

   
    private MovementCategoryEntity CreateMovementCategory( 
        string name,
        string description,
        string creator = "system",
        BaseDescriptorEntity? descriptor = null,
        MovementCategoryId? parentId = null,
        params eMovementCategories[] baseCategories)
    {
        var content = new MovementCategoryContent(name, baseCategories);
        content.ParentCategoryId = parentId;
        
        var auditInfo = GetDefaultAudit(creator);

        descriptor ??= CreateBaseDescriptor(description, auditInfo);
        
        var cat = _movementCategoryFactory.Create(new MovementCategoryCreationConfig
        {
             Id = MovementCategoryId.New(),
             Name = name,
             BaseCategories = baseCategories,
             Audit = auditInfo,
             Descriptor = descriptor
        });

        return cat;
    }

    private TEntity GetOrCreateEntity<TEntity>(string name, Func<TEntity> factory)
        where TEntity : class
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Entity name cannot be empty.", nameof(name));
        }

        var key = name.Trim();
        if (_resolver.TryGet<TEntity>(key, out var existing))
        {
            return existing;
        }

        var entity = factory();
        _resolver.Track(key, entity);
        return entity;
    }

    internal bool TryGetMovement(string name, out MovementEntity entity)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            entity = null!;
            return false;
        }

        return _resolver.TryGet(name, out entity);
    }

    internal void TrackMovement(MovementEntity movement)
    {
        if (movement == null)
        {
            throw new ArgumentNullException(nameof(movement));
        }

        var movementName = movement.Content?.Name;
        if (string.IsNullOrWhiteSpace(movementName))
        {
            throw new InvalidOperationException("Movement name is required to track movement.");
        }

        _resolver.Track(movementName, movement);
    }


   
    public MovementEntity GetOrCreateMovement(string name, string description, string creator, BaseDescriptorEntity? descriptor, string categoryName, List<string> equipmentsName, List<string> mainMusclesNames, List<string> secondaryMusclesNames, string? variantOfMovementName = null)
    {
        return GetOrCreateEntity(name, () =>
            CreateMovementFromNames(name, description, creator, descriptor, categoryName, equipmentsName ,mainMusclesNames, secondaryMusclesNames, variantOfMovementName));

        
    }

    private MovementEntity CreateMovementFromNames(
        string name,
        string description,
        string creator,
        BaseDescriptorEntity? descriptor,
        string categoryName,
        List<string> equipmentsName,
        List<string> mainMusclesNames,
        List<string> secondaryMusclesNames,
        string? variantOfMovementName)
    {
        // resolve category, equipment, and muscles (creates placeholders when missing)
        var category = GetOrCreateMovementCategory(categoryName, "");

        var equipments = equipmentsName
            .Select(e => GetOrCreateEquipment(e, ""))
            .ToList();

      
        
        var mainMuscles = mainMusclesNames
            .Select(m => GetOrCreateMuscle(m, "", "", eBodySection.undefined))
            .ToList();
        var secondaryMuscles = secondaryMusclesNames
            .Select(m => GetOrCreateMuscle(m, "", "", eBodySection.undefined))
            .ToList();

        _logger.Log(nameof(EntityFactory), $"Movement: {name} -  " +
                                           $"| Got or Created {equipments.Count} equipments" +
                                           $"| Got or Created {mainMuscles.Count} Main Muscles" +
                                           $"| Got or Created {secondaryMuscles.Count} Secondary Muscles");

        
        var auditInfo = GetDefaultAudit(creator);

        descriptor ??= CreateBaseDescriptor(description, auditInfo);

        var movementContent = new MovementContent(
            name,
            category.Id,
            new MuscleWorked(mainMuscles, secondaryMuscles),
            new EquipmentIdList(equipments),
            variantOf: (variantOfMovementName, null)); // variant resolved post creation

        var persistence = BuildMovementPersistenceModel(
            category,
            equipments,
            mainMuscles,
            secondaryMuscles);

        _logger.Log(nameof(EntityFactory), $"Created movement {name} : " +
                                           $"muscle worked : primary {movementContent.MusclesWorked.PrimaryMuscles.Count()}" +
                                           $"| equipment : {movementContent.EquipmentRequired.Count()}");
        
        return _movementFactory.Create(movementContent, auditInfo, descriptor, persistence);
    }

    private static MovementPersistenceModel BuildMovementPersistenceModel(
        MovementCategoryEntity category,
        IReadOnlyCollection<EquipmentEntity> equipments,
        IReadOnlyCollection<MuscleEntity> mainMuscles,
        IReadOnlyCollection<MuscleEntity> secondaryMuscles)
    {
        if (category.DbId <= 0)
        {
            throw new InvalidOperationException(
                $"Movement category '{category.Content.Name}' must exist in the database before seeding a movement.");
        }

        var muscleLookup = new Dictionary<MuscleId, int>();
        foreach (var muscle in mainMuscles.Concat(secondaryMuscles))
        {
            if (muscle.DbId <= 0)
            {
                throw new InvalidOperationException(
                    $"Muscle '{muscle.Content.Name}' must exist in the database before seeding a movement.");
            }

            muscleLookup[muscle.Id] = muscle.DbId;
        }

        var equipmentLookup = new Dictionary<EquipmentId, int>();
        foreach (var equipment in equipments)
        {
            if (equipment.DbId <= 0)
            {
                throw new InvalidOperationException(
                    $"Equipment '{equipment.Content.Name}' must exist in the database before seeding a movement.");
            }

            equipmentLookup[equipment.Id] = equipment.DbId;
        }

        return new MovementPersistenceModel(
            category.DbId,
            muscleLookup,
            equipmentLookup);
    }
    
}
