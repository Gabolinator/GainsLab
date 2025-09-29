using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;


namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;


public class MuscleContent : IEntityContent<MuscleContent>
{
    public string Name { get; set; }
    public eBodySection BodySection { get; set; } = eBodySection.undefined;
   
    public MuscleContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Muscle name is required.", nameof(Name));
        return this;
    }
}

 

public class MuscleEntity :  EntityBase<MuscleId, MuscleContent, AuditedInfo>, IDescribed<BaseDescriptorEntity>
{
    
    public MuscleEntity(
        MuscleContent content, 
        MuscleId id, 
        AuditedInfo creation, 
        BaseDescriptorEntity descriptor, 
        IEnumerable<MuscleId>? antagonists = null,
        int dbId = -1) : base(id, content,creation, dbId)
    {
        Descriptor = descriptor;
        AntagonistIds = new HashSet<MuscleId>(antagonists ?? Array.Empty<MuscleId>());
    }
    
    public override EntityType Type => EntityType.Muscle;
    
    public MuscleId Id { get; }
    public MuscleContent Content { get; }

    public IReadOnlySet<MuscleId> AntagonistIds { get; }
    
    public AuditedInfo CreationInfo { get; }
    public BaseDescriptorEntity Descriptor { get; }

    public MuscleEntity WithAntagonists(IEnumerable<MuscleId>? ids)
    {
        var muscle = new MuscleEntity(Content, Id, CreationInfo, Descriptor ,ids, DbId);
        return muscle;
    }
      
    
}

