
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;
using GainsLab.Models.Logging;


namespace GainsLab.Models.DataManagement.Caching;

public class EquipmentListCache : BaseComponentCache<EquipmentList>
{
    public EquipmentListCache(IWorkoutLogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.EquipmentList;
}