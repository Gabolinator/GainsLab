using GainsLab.Application.Interfaces;
using GainsLab.Domain;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.EntityFactory;

public class MovementCategoryCreationConfig
{
    public MovementCategoryId? Id { get; init; }

    public string Name { get; init; }

    public MovementCategoryId? ParentId { get; init; }
    public AuditedInfo? Audit { get; init; }
    public BaseDescriptorEntity? Descriptor { get; init; }
    public eMovementCategories[]? BaseCategories { get; init; } = null;
        

    public string GetCreatedBy() => 
        Audit !=null ? 
            Audit.CreatedBy : !string.IsNullOrWhiteSpace(CreatedBy) ? 
                CreatedBy : "system";

    public string? CreatedBy { get; set; }
}


public class MovementCategoryFactory: IEntityFactory<MovementCategoryEntity, MovementCategoryCreationConfig>
{
    public MovementCategoryFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descSvc, IEntitySeedResolver resolver)
    {
        _clock = clock;
        _descSvc = descSvc;
        _resolver = resolver;
    }
    
    private readonly IClock _clock;              
    private readonly IDescriptorService<BaseDescriptorEntity> _descSvc;
    private readonly IEntitySeedResolver _resolver;

    public MovementCategoryEntity Create(MovementCategoryCreationConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        if (string.IsNullOrWhiteSpace(config.Name)) throw new ArgumentNullException(nameof(config.Name));
        
        
        var content = new MovementCategoryContent(config.Name, config.BaseCategories ?? []);
        content.Validate();
        
        var id = config.Id ?? MovementCategoryId.New();
        var audit = config.Audit ?? AuditedInfo.New(_clock.UtcNow, config.GetCreatedBy());
        var descriptor = config.Descriptor ?? _descSvc.CreateFor(id);
      
        return new MovementCategoryEntity(content, id, audit, descriptor).WithParentCategory(config.ParentId);
    }
}