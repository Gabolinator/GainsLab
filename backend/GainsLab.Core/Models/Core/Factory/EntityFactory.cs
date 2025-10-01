using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Utilities;

namespace GainsLab.Core.Models.Core.Factory;

public class EntityFactory
{


    private readonly EquipmentFactory _equipmentFactory = new();
    private readonly DescriptorFactory _descriptorFactory = new();
    private IClock Clock => CoreUtilities.Clock;
    
    
  

    public List<EquipmentEntity> CreateBaseEquipments()
    {

        var equipments = new List<EquipmentEntity>();
        
        var auditInfo = AuditedInfo.New(Clock.UtcNow, "system");

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

