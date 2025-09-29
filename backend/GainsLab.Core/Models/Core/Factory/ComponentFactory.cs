
using GainsLab.Models.Core;
using GainsLab.Models.DataManagement;
using GainsLab.Models.Logging;

using GainsLab.Models.WorkoutComponents;
using GainsLab.Models.WorkoutComponents.Equipment;
using GainsLab.Models.WorkoutComponents.Movement;
using GainsLab.Models.WorkoutComponents.MovementCategory;
using GainsLab.Models.WorkoutComponents.Muscle;

namespace GainsLab.Models.Factory;

public class ComponentFactory
{

    public ComponentFactory(ILogger logger, IDataManager dataManager)
    {
        _logger = logger;
        _dataManager = dataManager;
    }

    public readonly ILogger _logger;
    public readonly IDataManager _dataManager;
 
    public void CreateTestData()
    {

        
      //   var jumpRope = new Equipment("Jump Rope","jumpRope");
      //   
      //   _dataManager.SaveComponentAsync(jumpRope);
      //   _dataManager.SaveComponentAsync(jumpRope);
      //
      //   
      //   
      //   var quad = new Muscle("Quadriceps", "quad", eBodySection.LowerBody);
      //   
      //   var harmstring = new Muscle("Harmstring", "harmstring",  eBodySection.LowerBody);
      //   Muscle.SetAsAntagonists(quad, harmstring);
      //
      //   var muscleGroups = new MusclesGroup(quad.ToComponentList(), harmstring.ToComponentList());
      //   
      //   var bodyweight = new MovementCategory("BodyWeight", "bodyweight", eMovementCategories.BodyWeight);
      //
      //   var squat = new Movement("Squat", "squat");
      //
      //   var weightReps =
      //       new WorkloadCalculationProfile("Weight x Reps", "weightReps", eWorkloadCalculationType.Weight_Reps);
      //
      //   
      // //  _logger.Log(nameof(ComponentFactory),jumpRope.ToComponentList().ToString());
      //
      //   squat.AddMusclesGroup(muscleGroups);
      //   squat.AddMovementCategory(bodyweight);
      //   squat.AddWorkloadCalculationProfile(weightReps);
      //   squat.AddEquipmentList(jumpRope.ToComponentList());
      //
      //  // _logger.Log(nameof(ComponentFactory),squat.ToString());

       



    }

}