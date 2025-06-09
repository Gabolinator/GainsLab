
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;
using GainsLab.Models.Logging;


namespace GainsLab.Models.DataManagement.Caching;

public class EquipmentsCache : BaseComponentCache<Equipment>
{
    public EquipmentsCache(IWorkoutLogger logger)
    {
        _logger = logger;
    }

    public override eWorkoutComponents ComponentType => eWorkoutComponents.Equipment;
    
   
}