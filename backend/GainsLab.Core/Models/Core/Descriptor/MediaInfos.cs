namespace GainsLab.Core.Models.Core;


public class MediaItem
{
    
}

//will hold images or videos urls
public class MediaInfos
{
    public int DbId { get; private set; }
    public List<MediaItem> Notes { get; } = new();
    public void AddMedia(string text) => Notes.Add(new());
}