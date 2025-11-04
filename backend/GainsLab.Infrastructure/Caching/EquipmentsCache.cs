using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement.Caching;

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
