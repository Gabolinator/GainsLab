using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace GainsLab.Core.Models.Core.Entities.Identifier;

/// <summary>
/// Maintains a unique set of equipment identifiers for relationship tracking.
/// </summary>
public class EquipmentIdList : IEnumerable<EquipmentId>
{
    public List<EquipmentId> Ids { get; set; } = new();

    public IEnumerator<EquipmentId> GetEnumerator() => Ids.GetEnumerator();

    /// <summary>
    /// Adds a single identifier when it is not already present.
    /// </summary>
    public void AddUnique(EquipmentId id)
    {
        if (!Ids.Contains(id))
            Ids.Add(id);
    }
    
    /// <summary>
    /// Adds distinct identifiers from the supplied sequence.
    /// </summary>
    public void AddUniques(IEnumerable<EquipmentId>? ids)
    {
        if (ids is null) return;

        // Fast path when empty: just add distinct incoming IDs
        if (Ids.Count == 0)
        {
            Ids.AddRange(ids.Distinct());
            return;
        }

        // Use a set to avoid O(n*m) Contains checks
        var existing = new HashSet<EquipmentId>(Ids);
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
