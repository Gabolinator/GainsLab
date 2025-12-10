
using GainsLab.Domain;
using GainsLab.Infrastructure.Utilities;

namespace GainsLab.Infrastructure.DB.Outbox;

public static class OutboxExtension
{
    /// <summary>
    /// Attempts to derive the <see cref="EntityType"/> from the serialized payload.
    /// </summary>
    /// <param name="change">The outbox change containing the serialized payload.</param>
    /// <param name="entityType">Populated with the resolved entity type when successful.</param>
    public static bool TryResolveEntityType(this OutboxChangeRecord change, out EntityType entityType)
    {
        entityType = EntityType.unidentified;
        if (!Enum.TryParse(change.Entity, out DTOEntityType dtoType))
        {
            CoreUtilities.Logger.LogWarning(nameof(OutboxExtension),
                $"Could not parse {change.Entity} to DTOEntityType");
            return false;
        }

        entityType = dtoType.GetEntityTye();

        return entityType != EntityType.unidentified;
    }
}