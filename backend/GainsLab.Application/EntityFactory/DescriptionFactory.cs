using GainsLab.Application.Interfaces;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.EntityFactory;

/// <summary>
/// Configuration payload used when creating descriptor entities.
/// </summary>
public sealed class DescriptiorCreationConfig
{

    public DescriptorId? Id { get; init; }

    public AuditedInfo? Audit { get; init; }

    public BaseDescriptorContent Content { get; init; } = default!;

    public string GetCreatedBy() => 
        Audit !=null ? 
            Audit.CreatedBy : !string.IsNullOrWhiteSpace(CreatedBy) ? 
                CreatedBy : "system";

    public string? CreatedBy { get; set; }
}

/// <summary>
/// Builds descriptor entities while coordinating audit metadata and persistence hooks.
/// </summary>
public class DescriptorFactory : IEntityFactory<BaseDescriptorEntity, DescriptiorCreationConfig>
{
    public  DescriptorFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descSvc, IEntitySeedResolver resolver)
    {
        _clock = clock;
        _descSvc = descSvc;
        _resolver = resolver;
    }
    
    
    private readonly IClock _clock;              
    private readonly IDescriptorService<BaseDescriptorEntity> _descSvc;
    private readonly IEntitySeedResolver _resolver;

    /// <summary>
    /// Creates and persists a descriptor from the supplied configuration.
    /// </summary>
    public BaseDescriptorEntity Create(DescriptiorCreationConfig cfg)
    {
        if (cfg is null) throw new ArgumentNullException(nameof(cfg));
        if (cfg.Content is null) throw new ArgumentNullException(nameof(cfg.Content));
        var content = cfg.Content.Validate(); 

        var id = cfg.Id ?? DescriptorId.New();
        
        var audit = cfg.Audit ?? AuditedInfo.New(_clock.UtcNow, cfg.GetCreatedBy());

        var descriptor = new BaseDescriptorEntity(id,content ,audit);
        
        _descSvc.Update(descriptor);

        return descriptor;
    }
}

