namespace GainsLab.Domain.Interfaces.Entity;

/// <summary>
/// Contract for content objects that can self-validate.
/// </summary>
public interface IEntityContent<TContent>
{
    public TContent Validate();

}
