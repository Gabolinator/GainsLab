namespace GainsLab.Domain.Interfaces;

/// <summary>
/// Represents a factory that can construct entities from configuration objects.
/// </summary>
public interface IEntityFactory<TEntity, TConfig>
{

    public TEntity Create (TConfig config);

   

}
