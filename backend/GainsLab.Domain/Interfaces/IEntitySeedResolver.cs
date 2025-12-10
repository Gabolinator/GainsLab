namespace GainsLab.Domain.Interfaces;

/// <summary>
/// Tracks already created domain entities during seeding so subsequent factories can reuse them.
/// </summary>
public interface IEntitySeedResolver
{
    /// <summary>
    /// Attempts to resolve a previously registered entity by its natural key.
    /// </summary>
    /// <typeparam name="TEntity">The domain entity type.</typeparam>
    /// <param name="key">Natural key (e.g., name) used to identify the entity.</param>
    /// <param name="entity">Resolved entity when found.</param>
    /// <returns><c>true</c> when the entity was already registered.</returns>
    bool TryGet<TEntity>(string key, out TEntity entity)
        where TEntity : class;

    /// <summary>
    /// Registers the supplied entity so that future <see cref="TryGet{TEntity}"/> calls can reuse it.
    /// </summary>
    /// <typeparam name="TEntity">The domain entity type.</typeparam>
    /// <param name="key">Natural key (e.g., name) used to identify the entity.</param>
    /// <param name="entity">Entity instance to track.</param>
    void Track<TEntity>(string key, TEntity entity)
        where TEntity : class;

    /// <summary>
    /// Clears cached entries for the supplied entity type.
    /// </summary>
    /// <typeparam name="TEntity">Entity type to clear.</typeparam>
    void Clear<TEntity>();
}
