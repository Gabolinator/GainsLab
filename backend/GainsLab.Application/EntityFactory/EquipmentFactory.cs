using GainsLab.Application.Interfaces;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.EntityFactory;


/// <summary>
/// Configuration payload used when creating equipment entities.
/// </summary>
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

/// <summary>
/// Builds equipment entities and wires up descriptor dependencies.
/// </summary>
public class EquipmentFactory : IEntityFactory<EquipmentEntity, EquipmentCreationConfig>
{
    public EquipmentFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descSvc, IEntitySeedResolver resolver)
    {
        _clock = clock;
        _descSvc = descSvc;
        _resolver = resolver;
    }
    
    private readonly IClock _clock;              
    private readonly IDescriptorService<BaseDescriptorEntity> _descSvc;
    private readonly IEntitySeedResolver _resolver;

    public (bool existed, EquipmentEntity entity) GetOrCreate(EquipmentCreationConfig cfg)
    {
        if (cfg is null) throw new ArgumentNullException(nameof(cfg));
        if (cfg.Content is null) throw new ArgumentNullException(nameof(cfg.Content));
        var content = cfg.Content.Validate();

        var key = content.Name?.Trim();
        if (string.IsNullOrWhiteSpace(key))
        {
            throw new ArgumentException("Equipment name is required to resolve existing entities.", nameof(cfg));
        }

        if (_resolver.TryGet<EquipmentEntity>(key, out var existing))
        {
            return (true, existing);
        }

        var created = Create(cfg);
        _resolver.Track(key, created);
        return (false, created);
    }


    /// <summary>
    /// Creates an equipment entity from the supplied configuration.
    /// </summary>
    public EquipmentEntity Create(EquipmentCreationConfig cfg)
    {
        
        // Validate early
        if (cfg.Content is null) throw new ArgumentNullException(nameof(cfg.Content));
        var content = cfg.Content.Validate();
        
        var id = cfg.Id ?? EquipmentId.New();
        var audit = cfg.Audit ?? AuditedInfo.New(_clock.UtcNow, cfg.GetCreatedBy());
        var descriptor = cfg.Descriptor ?? _descSvc.CreateFor(id);

        return new EquipmentEntity(content, id, audit, descriptor);
    }

        
}
