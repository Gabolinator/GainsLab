namespace GainsLab.Models.Core;

public record Description(string? Text, IIdentifier? Identifier)
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(Text) && Identifier == null;

    public Description Copy()
    {
        return new Description(Text, Identifier);
    }

    public override string ToString()
        => $"Description: {(string.IsNullOrWhiteSpace(Text) ? "None" : Text)}"
           + (Identifier != null ? $", ID: {Identifier}" : "");
}

public record EmptyDescription() : Description(null, null)
{
    public override string ToString() => "Description: (empty)";
}