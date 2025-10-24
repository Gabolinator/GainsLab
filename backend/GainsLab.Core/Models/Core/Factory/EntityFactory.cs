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

    }

    private readonly ILogger _logger;
    private readonly EquipmentFactory _equipmentFactory;
    private readonly DescriptorFactory _descriptorFactory;
    private readonly IClock _clock;


    /// <summary>
    /// Generates the seeded equipment entities required for a new deployment.
    /// </summary>
    public List<EquipmentEntity> CreateBaseEquipments()
    {
        
        var equipments = new List<EquipmentEntity>();
        
        var auditInfo = AuditedInfo.New(_clock.UtcNow, "system");

        var jumpRope = _equipmentFactory.Create(new EquipmentCreationConfig
        {
            Id = EquipmentId.New(),
            Content = new EquipmentContent("Jump Rope"),
            Audit = auditInfo,
            Descriptor = _descriptorFactory.Create(new DescriptiorCreationConfig
            {
               Id = DescriptorId.New(),
               Content = new BaseDescriptorContent
               {
                   Description = Description.New("Some description for jump rope")
               },
               Audit = auditInfo
            })
        });


        equipments.Add(jumpRope);

        return equipments;
    }

  
}

