using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;


namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;



public record EquipmentContent( string Name) : IEntityContent<EquipmentContent>
{
    
    public EquipmentContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Equipment name is required.", nameof(Name));
        return this;
    }
}

public class EquipmentEntity : EntityBase<EquipmentId, EquipmentContent, AuditedInfo>, IDescribed<BaseDescriptorEntity>
{
    
    public  EquipmentEntity(
        EquipmentContent content, 
        EquipmentId id, 
        AuditedInfo creation, 
        BaseDescriptorEntity descriptor, 
        int dbId = -1) : base(id, content, creation,dbId )
    {
        
         Descriptor = descriptor;
    }
    
    public  EquipmentEntity(EquipmentContent content, string createdBy, BaseDescriptorEntity? descriptor = null, int dbId = -1)
        : base(EquipmentId.New(), content.Validate(), AuditedInfo.New(createdBy), dbId)
    {
        Descriptor = descriptor ?? new BaseDescriptorEntity();
    }
    
    public override EntityType Type => EntityType.Equipment;
    public BaseDescriptorEntity Descriptor { get; }
    public string Name => Content.Name;
}