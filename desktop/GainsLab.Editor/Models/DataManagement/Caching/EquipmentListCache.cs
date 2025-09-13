
using GainsLab.Models.Core;
using GainsLab.Models.Logging;
using GainsLab.Models.WorkoutComponents.Equipment;


namespace GainsLab.Models.DataManagement.Caching;

public class EquipmentListCache : BaseComponentCache<EquipmentList>
{
    public EquipmentListCache(ILogger logger)
    {
        _logger = logger;
    }
    public override eWorkoutComponents ComponentType => eWorkoutComponents.EquipmentList;
}