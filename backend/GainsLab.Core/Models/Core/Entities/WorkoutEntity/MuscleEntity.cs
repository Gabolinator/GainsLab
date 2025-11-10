using System;
using System.Collections.Generic;
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
        AntagonistIds = new HashSet<MuscleId>(antagonists ?? Array.Empty<MuscleId>());
    }
    
    public override EntityType Type => EntityType.Muscle;
    
    public MuscleId Id { get; }
    public MuscleContent Content { get; }

    public IReadOnlySet<MuscleId> AntagonistIds { get; set; }

    public AuditedInfo CreationInfo { get; }
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
        var added = AddAntagonist(antagonists.Select(a=>a.Id).ToArray());
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
        if (AntagonistIds == null || AntagonistIds.Count == 0)
        {
            AntagonistIds = new HashSet<MuscleId>(antagonists);
            return AntagonistIds;
        }

        var list = new List<MuscleId>(AntagonistIds);
        
        foreach (var antagonist in antagonists)
        {
            if(list.Contains(antagonist)) continue;
            list.Add(antagonist);
        }

        AntagonistIds = list.ToHashSet();
        return AntagonistIds;
    }
    
}

