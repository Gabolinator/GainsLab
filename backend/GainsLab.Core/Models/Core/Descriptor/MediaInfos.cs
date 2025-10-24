using System.Collections.Generic;

namespace GainsLab.Core.Models.Core;

/// <summary>
/// Represents a media element (image, video, etc.) linked to a descriptor.
/// </summary>
public class MediaItem
{
    
}

/// <summary>
/// Collection of media assets attached to workout content.
/// </summary>
public class MediaInfos
{
    public int DbId { get; private set; }
    public List<MediaItem> Notes { get; } = new();
    public void AddMedia(string text) => Notes.Add(new());
}
