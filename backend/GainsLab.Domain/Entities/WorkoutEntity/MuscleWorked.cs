using System.Collections;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Domain.Entities.WorkoutEntity;

/// <summary>
/// Describes the primary and secondary muscles engaged by a movement.
/// </summary>
public class MuscleWorked
{

    public MuscleWorked()
    {
    }
    public MuscleWorked(IEnumerable<MuscleEntity> main, IEnumerable<MuscleEntity> secondary)
    {
        PrimaryMuscles = new MuscleIdList(main.Select(m => m.Id));
        SecondaryMuscles = new MuscleIdList(secondary.Select(m => m.Id));
    }
    

    public MuscleIdList PrimaryMuscles { get; set; }
    public MuscleIdList SecondaryMuscles { get; set; }

    private MuscleIdList? AllMuscles { get; set; } = null;
    
    public MuscleIdList GetAllMuscle()
    {
       return AllMuscles ??= new MuscleIdList(ContatMuscles()) ;
        
    }

    private IEnumerable<MuscleId> ContatMuscles()
    {
        var list = new MuscleIdList(PrimaryMuscles);
        list.AddUniques(SecondaryMuscles);
        return list;
    }
}

/// <summary>
/// Maintains a unique collection of muscle identifiers.
/// </summary>
public class MuscleIdList : IEnumerable<MuscleId>
{
    public MuscleIdList()
    {
    }
    
    public MuscleIdList(IEnumerable<MuscleId> ids)
    {
        AddUniques(ids);
    }
    
    

    public List<MuscleId> Ids { get; set; } = new();

    public IEnumerator<MuscleId> GetEnumerator() => Ids.GetEnumerator();

    /// <summary>
    /// Adds a single identifier when it is not already present.
    /// </summary>
    public void AddUnique(MuscleId id)
    {
        if (!Ids.Contains(id))
            Ids.Add(id);
    }
    
    /// <summary>
    /// Adds distinct identifiers from the supplied sequence.
    /// </summary>
    public void AddUniques(IEnumerable<MuscleId>? ids)
    {
        if (ids is null) return;

        // Fast path when empty: just add distinct incoming IDs
        if (Ids.Count == 0)
        {
            Ids.AddRange(ids.Distinct());
            return;
        }

        // Use a set to avoid O(n*m) Contains checks
        var existing = new HashSet<MuscleId>(Ids);
        foreach (var id in ids)
        {
            if (existing.Add(id)) // true if it wasn't present
                Ids.Add(id);
        }
    }
    
    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
