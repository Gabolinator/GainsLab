namespace GainsLab.Core.Models.Core.Interfaces.Entity;

public interface IEntityContent<TContent>
{
    public TContent Validate();

}