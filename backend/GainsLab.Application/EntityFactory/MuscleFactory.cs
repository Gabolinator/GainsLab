using GainsLab.Application.Interfaces;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.EntityFactory;


/// <summary>
/// Configuration payload used when creating muscle entities.
/// </summary>
public sealed class MuscleCreationConfig
{
 public MuscleId? Id { get; init; }
 public MuscleContent Content { get; init; } = default!;
 public AuditedInfo? Audit { get; init; }
 public BaseDescriptorEntity? Descriptor { get; init; }
 public IEnumerable<MuscleId>? Antagonists { get; init; }


 public string GetCreatedBy() => 
  Audit !=null ? 
   Audit.CreatedBy : !string.IsNullOrWhiteSpace(CreatedBy) ? 
    CreatedBy : "system";

 public string? CreatedBy { get; set; }
    
}

public class MuscleFactory :  IEntityFactory<MuscleEntity, MuscleCreationConfig>
{
    public MuscleFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descriptorService, IEntitySeedResolver resolver)
    {
        _clock = clock;
        _descriptorService = descriptorService;
        _resolver = resolver;
    }

    private readonly IClock _clock;
    private readonly IDescriptorService<BaseDescriptorEntity> _descriptorService;
    private readonly IEntitySeedResolver _resolver;

    public MuscleEntity Create(MuscleCreationConfig config)
    {
        if (config is null) throw new ArgumentNullException(nameof(config));
        if (config.Content is null) throw new ArgumentNullException(nameof(config.Content));

        var content = config.Content.Validate();
        var id = config.Id ?? MuscleId.New();
        var audit = config.Audit ?? AuditedInfo.New(_clock.UtcNow, config.GetCreatedBy());
        var descriptor = config.Descriptor ?? _descriptorService.CreateFor(id);
        var antagonists = config.Antagonists ?? Array.Empty<MuscleId>();

        return new MuscleEntity(content, id, audit, descriptor, antagonists);
    }
}
