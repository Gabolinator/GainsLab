namespace GainsLab.Models.Core.Descriptor;

public record Notes(string? Text, WorkoutComponentIdentifier Identifier)
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(Text) && Identifier.IsEmpty();

    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }

    public Notes() : this("", new EmptyWorkoutComponentIdentifier())
    {
        
    }

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