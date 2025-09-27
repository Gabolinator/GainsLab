
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.Equipment;


namespace GainsLab.Models.DataManagement.Caching;

public class EquipmentsCache : BaseComponentCache<Equipment>
{
    public EquipmentsCache(ILogger logger)
    {
        _logger = logger;
    }

    public override eWorkoutComponents ComponentType => eWorkoutComponents.Equipment;
    
   
}