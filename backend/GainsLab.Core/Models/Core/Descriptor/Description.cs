namespace GainsLab.Core.Models.Core.Descriptor;

public record Description(string? Text)
{
    public override string ToString() =>
        string.IsNullOrWhiteSpace(Text) ? "Notes: None" : $"Notes: {Text}";
    
}
// {
//     public bool IsEmpty() => string.IsNullOrWhiteSpace(Text) && Identifier.IsEmpty();
//
//     public int Id
//     {
//         get => Identifier.DbID ?? -1;
//
//         set => Identifier.DbID = value;
//     }
//     
//     public Description() : this("", new EmptyWorkoutComponentIdentifier())
//     {
//     }
//
//     public Description Copy()
//     {
//         return new Description(Text, Identifier);
//     }
//
//     public override string ToString()
//         => $"Description: {(string.IsNullOrWhiteSpace(Text) ? "None" : Text)}"
//            + (Identifier != null ? $", ID: {Identifier}" : "");
// }
//
// public record EmptyDescription() : Description(null, null)
// {
//     public override string ToString() => "Description: (empty)";
// }