namespace GainsLab.Core.Models.Core.Interfaces.DB;

public interface IDto
{
    public int Iid { get; }
    public Guid  Iguid { get;  }

    public EntityType Type { get; }

}