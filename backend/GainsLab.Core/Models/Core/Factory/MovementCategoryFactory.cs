using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Interfaces.Factory;

namespace GainsLab.Core.Models.Core.Factory;

public class MovementCategoryCreationConfig
{
    public MovementCategoryId? Id { get; init; }
    public MovementCategoryContent Content { get; init; } = default!;
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
    public MovementCategoryFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descSvc)
    {
        _clock = clock;
        _descSvc = descSvc;
    }
    
    private readonly IClock _clock;              
    private readonly IDescriptorService<BaseDescriptorEntity> _descSvc;
    
    public MovementCategoryEntity Create(MovementCategoryCreationConfig config)
    {
        throw new NotImplementedException();
    }
}