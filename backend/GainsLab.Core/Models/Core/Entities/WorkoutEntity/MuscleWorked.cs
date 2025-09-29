using System.Collections;
using GainsLab.Core.Models.Core.Entities.Identifier;

namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;

public class MuscleWorked
{
    public MuscleIdList MainMuscles { get; set; }
    public MuscleIdList SecondaryMuscles { get; set; }
}

public class MuscleIdList : IEnumerable<MuscleId>
{
    public List<MuscleId> Ids { get; set; } = new();

    public IEnumerator<MuscleId> GetEnumerator() => Ids.GetEnumerator();

    public void AddUnique(MuscleId id)
    {
        if (!Ids.Contains(id))
            Ids.Add(id);
    }
    
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