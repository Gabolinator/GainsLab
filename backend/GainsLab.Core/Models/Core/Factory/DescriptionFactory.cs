using System;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Interfaces.Factory;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Core.Models.Core.Factory;

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

