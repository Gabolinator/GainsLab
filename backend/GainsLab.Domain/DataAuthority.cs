namespace GainsLab.Domain;

/// <summary>
/// Indicates which tier is allowed to mutate a synchronized entity.
/// </summary>
public enum DataAuthority
{
    Upstream = 0,
    Downstream = 1,
    Bidirectional = 2
}
