using System;
using System.Collections.Generic;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Interfaces.Factory;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Core.Models.Core.Factory;


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
    public MuscleFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descriptorService)
    {
        _clock = clock;
        _descriptorService = descriptorService;
    }

    private readonly IClock _clock;
    private readonly IDescriptorService<BaseDescriptorEntity> _descriptorService;

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
