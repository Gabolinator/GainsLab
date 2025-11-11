using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;


namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;


/// <summary>
/// Mutable content describing a muscle and additional metadata.
/// </summary>
public class MuscleContent : IEntityContent<MuscleContent>
{
    public string Name { get; set; }

    public string LatinName { get; set; }
    public eBodySection BodySection { get; set; } = eBodySection.undefined;
   
    public MuscleContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Muscle name is required.", nameof(Name));
       // if (string.IsNullOrWhiteSpace(LatinName)) throw new ArgumentException("Latin name is required.", nameof(LatinName));
        if (BodySection == eBodySection.undefined) throw new ArgumentException("Body section must be specified.", nameof(BodySection));
        return this;
    }
}
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
