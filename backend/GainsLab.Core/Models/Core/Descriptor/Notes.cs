namespace GainsLab.Models.Core.Descriptor;

public record Notes(string? Text)
{
    public Notes Copy()
    {
        return new Notes(Text);
    }

    public override string ToString() =>
        string.IsNullOrWhiteSpace(Text) ? "Notes: None" : $"Notes: {Text}";
}

