
using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;


namespace GainsLab.Infrastructure.Caching;

/// <summary>
/// In-memory cache used to store equipment entities keyed by <see cref="EquipmentId"/>.
/// </summary>
public class EquipmentsCache : BaseComponentCache<EquipmentId,EquipmentEntity>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="EquipmentsCache"/> class.
    /// </summary>
    /// <param name="logger">Logger used for cache diagnostics.</param>
    public EquipmentsCache(ILogger logger)
    {
        _logger = logger;
    }

    /// <inheritdoc />
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Equipment;
    
   
}
