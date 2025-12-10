namespace GainsLab.Domain.Entities.Descriptor;

/// <summary>
/// Holds lightweight annotation text for a component.
/// </summary>
public record Notes(string? Text)
{
    /// <summary>
    /// Creates a copy of the current notes instance.
    /// </summary>
    public Notes Copy()
    {
        return new Notes(Text);
    }

    /// <summary>
    /// Returns a human-readable representation of the attached notes.
    /// </summary>
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Text) ? "Notes: None" : $"Notes: {Text}";
}

