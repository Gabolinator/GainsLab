using System.Collections;
using System.Collections.Generic;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Core.Models.Core.Factory;

/// <summary>
/// Coordinates creation of core domain entities and their descriptors.
/// </summary>
public class EntityFactory
{

    public EntityFactory(IClock clock, ILogger logger, IDescriptorService<BaseDescriptorEntity> descriptorService)
    {
        _logger = logger;
        _clock = clock;
        _equipmentFactory = new EquipmentFactory(clock, descriptorService );
        _descriptorFactory = new DescriptorFactory(clock, descriptorService );
        _muscleFactory = new MuscleFactory(clock, descriptorService);

    }

    private readonly ILogger _logger;
    private readonly EquipmentFactory _equipmentFactory;
    private readonly DescriptorFactory _descriptorFactory;
    private readonly MuscleFactory _muscleFactory;
    private readonly IClock _clock;


    /// <summary>
    /// Generates the seeded equipment entities required for a new deployment.
    /// </summary>
    public List<EquipmentEntity> CreateBaseEquipments()
    {
        
        var equipments = new List<EquipmentEntity>();

        var jumpRope =CreateEquipment("Jump Rope", "Some description for jump rope");
        
       

        equipments.Add(jumpRope);

        return equipments;
    }


    public AuditedInfo GetDefaultAudit(string creator = "system")
    {
       return AuditedInfo.New(_clock.UtcNow, creator);
    }

    public EquipmentEntity CreateEquipment(string name, string description ,string creator = "system")
    {
        var auditInfo = GetDefaultAudit(creator);

        var jumpRope = _equipmentFactory.Create(new EquipmentCreationConfig
        {
            Id = EquipmentId.New(),
            Content = new EquipmentContent(name),
            Audit = auditInfo,
            Descriptor = CreateBaseDescriptor(description, auditInfo)
        });

        return jumpRope;
    }

    public IEnumerable<MuscleEntity> CreateBaseMuscles()
    {

        var list = new List<MuscleEntity>();
        
        MuscleEntity quad = CreateMuscle("Quadriceps", "latin name for quad", "Some description for quad", eBodySection.LowerBody);
        MuscleEntity harmstring= CreateMuscle("Harmstring", "latin name for armstring", "Some description for harmstring", eBodySection.LowerBody);

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
}

