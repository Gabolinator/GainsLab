namespace GainsLab.Core.Models.Core.Interfaces.Factory;

public interface IEntityFactory<TEntity, TConfig>
{

    public TEntity Create (TConfig config);

   

}