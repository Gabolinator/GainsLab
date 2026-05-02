using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity.EntityContent;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity;

/// <summary>
/// Aggregate root representing a muscle along with descriptor data and antagonists.
/// </summary>
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
        _antagonistIds = new HashSet<MuscleId>(antagonists ?? Array.Empty<MuscleId>());
    }
    
    public override EntityType Type => EntityType.Muscle;
    private readonly HashSet<MuscleId> _antagonistIds;
    public IReadOnlySet<MuscleId> AntagonistIds => _antagonistIds;

    public BaseDescriptorEntity Descriptor { get; }

    /// <summary>
    /// Returns a copy with the provided set of antagonist identifiers.
    /// </summary>
    public MuscleEntity WithAntagonists(IEnumerable<MuscleId>? ids)
    {
        var muscle = new MuscleEntity(Content, Id, CreationInfo, Descriptor ,ids, DbId);
        return muscle;
    }


    public void AddAntagonist( bool mutualAdd , params MuscleEntity[] antagonists)
    {
        var added = AddAntagonist(antagonists
            .Where(a => a.Id != Id)
            .Select(a=>a.Id)
            .ToArray());
        if(!added.Any() || !mutualAdd) return;

        foreach (var muscleId in added)
        {
            var a = antagonists.FirstOrDefault(a => a.Id == muscleId);
            if(a ==null) continue;

            a.AddAntagonist(false, this);
        }
        
    }
    
    public IEnumerable<MuscleId> AddAntagonist(params MuscleId[] antagonists)
    {
        foreach (var antagonist in antagonists)
        {
            if (antagonist == Id) continue;
            _antagonistIds.Add(antagonist);
        }

        return _antagonistIds;
    }


    public override string ToString()
    {
        return $"Muscle entity : {Id} - Name : {Content.Name} - BodySection: {Content.BodySection}";
    }
}
