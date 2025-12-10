using System.Collections;

namespace GainsLab.Domain.Entities.Identifier;

/// <summary>
/// Maintains a unique set of movement identifiers for relationship tracking.
/// </summary>
public class MovementIdList  : IEnumerable<MovementId>
{
    public List<MovementId> Ids { get; set; } = new();

    public IEnumerator<MovementId> GetEnumerator() => Ids.GetEnumerator();

    /// <summary>
    /// Adds a single identifier when it is not already present.
    /// </summary>
    public void AddUnique(MovementId id)
    {
        if (!Ids.Contains(id))
            Ids.Add(id);
    }
    
    /// <summary>
    /// Adds distinct identifiers from the supplied sequence.
    /// </summary>
    public void AddUniques(IEnumerable<MovementId>? ids)
    {
        if (ids is null) return;

        // Fast path when empty: just add distinct incoming IDs
        if (Ids.Count == 0)
        {
            Ids.AddRange(ids.Distinct());
            return;
        }

        // Use a set to avoid O(n*m) Contains checks
        var existing = new HashSet<MovementId>(Ids);
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
