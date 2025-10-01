using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces.Factory;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Core.Models.Core.Factory;


public sealed class EquipmentCreationConfig
{
    public EquipmentId? Id { get; init; }
    public EquipmentContent Content { get; init; } = default!;
    public AuditedInfo? Audit { get; init; }
    public BaseDescriptorEntity? Descriptor { get; init; }

    public string GetCreatedBy() => 
        Audit !=null ? 
            Audit.CreatedBy : !string.IsNullOrWhiteSpace(CreatedBy) ? 
                CreatedBy : "system";

    public string? CreatedBy { get; set; }
    
}

public class EquipmentFactory : IEntityFactory<EquipmentEntity, EquipmentCreationConfig>
{
    private readonly IClock _clock;              
    private readonly IDescriptorService<BaseDescriptorEntity> _descSvc;
    
    
        public EquipmentEntity Create(EquipmentCreationConfig cfg)
        {
            // Validate early
            if (cfg.Content is null) throw new ArgumentNullException(nameof(cfg.Content));
            var content = cfg.Content.Validate(); // keep your Validate() pattern

            var id = cfg.Id ?? EquipmentId.New();
            var audit = cfg.Audit ?? AuditedInfo.New(_clock.UtcNow, cfg.GetCreatedBy());
            var descriptor = cfg.Descriptor ?? _descSvc.CreateFor(id);

            return new EquipmentEntity(content, id, audit, descriptor);
        }
    


}