namespace GainsLab.Core.Models.Core.Descriptor;

/// <summary>
/// Wraps rich textual information describing a workout component.
/// </summary>
public record Description(string? Text)
{
    
    /// <summary>
    /// Converts raw text into a <see cref="Description"/> instance.
    /// </summary>
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Text) ? "Notes: None" : $"Notes: {Text}";

    /// <summary>
    /// Factory helper that creates a descriptor from arbitrary text.
    /// </summary>
    public static Description New(string descriptionText)
    {
        return new Description(descriptionText);
    }
}


