using System.Collections;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core.Descriptor;

namespace GainsLab.Core.Models.Core.Descriptor;

/// <summary>
/// Provides collection semantics for descriptor tags while retaining database metadata.
/// </summary>
public class TagList : IEnumerable<Tag>
{

    public int DbId { get; private set; }
    public List<Tag> Tags { get; set; } = new();

    public TagList()
    {
        
    }
    
    public TagList(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
        {
           AddTag(tag);
        }
    }

    public void AddTag(Tag tag) =>Tags.Add(tag);
    
        
    

    public TagList Copy()
    {
        return new TagList(this);
    }

    public IEnumerator<Tag> GetEnumerator() => Tags.GetEnumerator();


    public override string ToString()
    {
        if (this.Count() == 0) return "Tags: (none)";
        return $"Tags: [{string.Join(", ", this)}]";
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}

