namespace GainsLab.Core.Models.Core.Interfaces;

/// <summary>
/// Represents content that can optionally expose nested data structures.
/// </summary>
public interface INestedContent<TNestedContent>
{
    public TNestedContent? NestedContent { get; }

}
