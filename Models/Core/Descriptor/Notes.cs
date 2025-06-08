namespace GainsLab.Models.Core;

public record Notes(string? Text, IIdentifier? Identifier)
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(Text) && Identifier == null;

    public Notes Copy()
    {
        return new Notes(Text, Identifier);
    }

    public override string ToString()
        => $"Notes: {(string.IsNullOrWhiteSpace(Text) ? "None" : Text)}"
           + (Identifier != null ? $", ID: {Identifier}" : "");
}

public record EmptyNotes() : Notes(null, null) 
{
    public override string ToString() => "Notes: (empty)";
}