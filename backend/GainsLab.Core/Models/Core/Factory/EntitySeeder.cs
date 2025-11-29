using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities.Logging;

namespace GainsLab.Core.Models.Core.Factory;

public class EntitySeeder
{
    private readonly EntityFactory _factory;
    private readonly ILogger _logger;
    
    public EntitySeeder(ILogger logger,EntityFactory factory)
    {
        _factory = factory;
        _logger = logger;
        
    }
    
    public IEnumerable<MuscleEntity> CreateBaseMuscles()
    {

        var list = new List<MuscleEntity>();
        
        MuscleEntity quad = _factory.GetOrCreateMuscle("Quadriceps", "latin name for quad", "Some description for quad", eBodySection.LowerBody);
        MuscleEntity harmstring= _factory.GetOrCreateMuscle("Harmstring", "latin name for armstring", "Some description for harmstring", eBodySection.LowerBody);

        quad.AddAntagonist(mutualAdd: true, harmstring);
        if (quad != null)
        {
            list.Add(quad);
            if(quad.Content != null)  _logger.Log(nameof(EntityFactory) , $"Added muscle  {quad.Content.Name} to list");
            else _logger.Log(nameof(EntityFactory) , $"Null content in muscle");
        }
       
        else _logger.Log(nameof(EntityFactory) , $"Muscle is null");
     
        list.Add(harmstring);
        _logger.Log(nameof(EntityFactory) , $"Added muscle {harmstring.Content.Name} to list");


        return list;

    }

    
    public IEnumerable<MovementEntity> CreateBaseMovements()
    {

        List<MovementEntity> movementEntities = new();
        MovementEntity squat = _factory.GetOrCreateMovement(
            name : "Squat",
            description : "Some Description For Squat",
            creator : "system", 
            descriptor:null,
            "Weightlifting",
           new List<string>{"Barbell"},
           new List<string>{"Quadriceps"},
           new List<string>{"Harmstring"},
            null);
        //
        // MovementEntity kettleBellSquat = _factory.GetOrCreateMovement(
        //     name : "Rack Squat",
        //     description : "Some Description For Rack Squat",
        //     creator : "system", 
        //     descriptor:null,
        //     "Kettlebelling",
        //     new List<string>{"KettleBell"},
        //     new List<string>{"Quadriceps"},
        //     new List<string>{"Harmstring"},
        //     "Squat");
        //
        
        movementEntities.AddRange(new []{squat});
        ResolveMovementVariants(movementEntities);

        return movementEntities;

    }
    
    public IEnumerable<MovementCategoryEntity> CreateBaseCategories()
    {


        List<MovementCategoryEntity> baseCategories = CreateMovementCategoryFromEnum();
        
    
        //then we create the custom categories
        var kettlebelling = _factory.GetOrCreateMovementCategory(
            "Kettlebelling", 
            "Some description for kettlebelling", 
            "system", 
            null,
            null, 
            eMovementCategories.Weightlifting, eMovementCategories.Cardio);
       
       
        var olympicWeightlifting = _factory.GetOrCreateMovementCategory(
            "Olympic WeightLifting", 
            "Some description for Olympic WeightLifting", 
            "system", 
            null,
            null, eMovementCategories.Weightlifting);

       
        baseCategories.AddRange(new List<MovementCategoryEntity>{kettlebelling, olympicWeightlifting});
       
       
        return baseCategories;

    }

    private List<MovementCategoryEntity> CreateMovementCategoryFromEnum()
    {
        var baseCategories = new List<MovementCategoryEntity>();
        
        //create the base categories for the enum values 
        foreach (var eMoveCat in Enum.GetValues<eMovementCategories>())
        {
            if(eMoveCat == eMovementCategories.undefined) continue;
            MovementCategoryEntity cat = _factory.GetOrCreateMovementCategory(eMoveCat.ToString(), eMoveCat.GetDescription(), "system", null, null, eMoveCat);
            baseCategories.Add(cat);
        }

        return baseCategories;
    }

    
    /// <summary>
    /// Generates the seeded equipment entities required for a new deployment.
    /// </summary>
    public List<EquipmentEntity> CreateBaseEquipments()
    {
        
        var equipments = new List<EquipmentEntity>();

        var jumpRope =_factory.GetOrCreateEquipment(
            "Jump Rope", 
            "Some description for jump rope");
        var kettlebell =_factory.GetOrCreateEquipment(
            "Kettle Bell", 
            "Some description for kettlebell");
        
        var barbel =_factory.GetOrCreateEquipment(
            "Barbell", 
            "Some description for barbell");
        
        equipments.AddRange(new List<EquipmentEntity>{jumpRope, kettlebell, barbel});

        return equipments;
    }

    private void ResolveMovementVariants(IList<MovementEntity> movementEntities)
    {
        for (var index = 0; index < movementEntities.Count; index++)
        {
            var movement = movementEntities[index];
            var (variantName, variantId) = movement.Content.variantOf;

            if (string.IsNullOrWhiteSpace(variantName) || variantId is not null)
            {
                continue;
            }

            if (!_factory.TryGetMovement(variantName, out var variantEntity))
            {
                throw new InvalidOperationException(
                    $"Movement '{movement.Content.Name}' references variant '{variantName}' that was not part of the seed payload.");
            }

            var resolvedMovement = movement.WithVariant(variantEntity.Id);
            movementEntities[index] = resolvedMovement;
            _factory.TrackMovement(resolvedMovement);

            _logger.Log(nameof(EntitySeeder),
                $"Resolved variant relation '{movement.Content.Name}' -> '{variantEntity.Content.Name}'.");
        }
    }
}
