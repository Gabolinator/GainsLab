using System.Collections.Generic;
using System.Diagnostics.Tracing;

namespace GainsLab.Models.Core;

public class TagList : List<Tag>
{

    public TagList()
    {
        
    }
    
    public TagList(IEnumerable<Tag> tags)
    {
        foreach (var tag in tags)
        {
           Add(tag);
        }
    }

    public TagList Copy()
    {
        return new TagList(this);
    }

    public override string ToString()
    {
        if (this.Count == 0) return "Tags: (none)";
        return $"Tags: [{string.Join(", ", this)}]";
    }
}

