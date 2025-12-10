namespace GainsLab.Domain.Interfaces;

/// <summary>
/// Abstraction over time retrieval to enable deterministic testing.
/// </summary>
public interface IClock
{
    DateTimeOffset UtcNow { get;}
}
