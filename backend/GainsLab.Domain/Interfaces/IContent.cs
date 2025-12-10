namespace GainsLab.Domain.Interfaces;

/// <summary>
/// Represents content that can optionally expose nested data structures.
/// </summary>
public interface INestedContent<TNestedContent>
{
    public TNestedContent? NestedContent { get; }

}
