
using GainsLab.Models.Core;
using GainsLab.Models.Core.WorkoutComponents;


namespace GainsLab.Models.DataManagement.Caching;

public class EquipmentsCache : BaseComponentCache<Equipment>
{
    public override eWorkoutComponents ComponentType => eWorkoutComponents.Equipment;
    
   
}