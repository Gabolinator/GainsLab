
using GainsLab.Domain.Interfaces;

namespace GainsLab.Infrastructure.DB;

/// <summary>
/// Simple in-memory resolver used while seeding entities so dependent factories can reuse them.
/// </summary>
public class EntitySeedResolver : IEntitySeedResolver
{
    private readonly ILogger _logger;
    private readonly Dictionary<Type, Dictionary<string, object>> _cache = new();
    private readonly object _gate = new();

    public EntitySeedResolver(ILogger logger)
    {
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <inheritdoc />
    public bool TryGet<TEntity>(string key, out TEntity entity)
        where TEntity : class
    {
        if (!TryNormalizeKey(key, out var normalized))
        {
            entity = null!;
            return false;
        }

        lock (_gate)
        {
            if (_cache.TryGetValue(typeof(TEntity), out var bucket) &&
                bucket.TryGetValue(normalized, out var value) &&
                value is TEntity typed)
            {
                entity = typed;
                return true;
            }
        }

        entity = null!;
        return false;
    }

    /// <inheritdoc />
    public void Track<TEntity>(string key, TEntity entity)
        where TEntity : class
    {
        if (entity == null) throw new ArgumentNullException(nameof(entity));
        if (!TryNormalizeKey(key, out var normalized))
        {
            throw new ArgumentException("Entity key cannot be empty.", nameof(key));
        }

        lock (_gate)
        {
            if (!_cache.TryGetValue(typeof(TEntity), out var bucket))
            {
                bucket = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
                _cache[typeof(TEntity)] = bucket;
            }

            bucket[normalized] = entity;
        }

        _logger.Log(nameof(EntitySeedResolver), $"Tracked {typeof(TEntity).Name} '{normalized}'.");
    }

    /// <inheritdoc />
    public void Clear<TEntity>()
    {
        lock (_gate)
        {
            _cache.Remove(typeof(TEntity));
        }
    }

    private static bool TryNormalizeKey(string key, out string normalized)
    {
        normalized = string.Empty;
        if (string.IsNullOrWhiteSpace(key))
        {
            return false;
        }

        normalized = key.Trim();
        return normalized.Length > 0;
    }
}
