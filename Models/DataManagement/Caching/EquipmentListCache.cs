
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;


namespace GainsLab.Models.DataManagement.Caching;

public class EquipmentListCache : BaseComponentCache<EquipmentList>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.EquipmentList;
}