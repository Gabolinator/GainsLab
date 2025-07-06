namespace GainsLab.Models.Core.Descriptor;

public record Description(string? Text, Identifier Identifier)
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(Text) && Identifier.IsEmpty();

    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public Description() : this("", new EmptyIdentifier())
    {
    }

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