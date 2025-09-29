using GainsLab.Core.Models.Core;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.WorkoutComponents.Equipment;
using GainsLab.Models.WorkoutComponents.MovementCategory;
using GainsLab.Models.WorkoutComponents.Muscle;

namespace GainsLab.Models.WorkoutComponents.Movement;

// public class Movement : ICompositeWorkoutComponent<MovementContent>
// {
//     
//     public Movement(ComponentDescriptor descriptor, WorkoutComponentIdentifier identifier)
//     {
//         Descriptor = descriptor;
//         Identifier = identifier;
//         Content = new MovementContent();
//     }
//     
//     public Movement(string name, string stringID)
//     {
//         Descriptor = new ComponentDescriptor(name,ComponentType);
//         Identifier = new WorkoutComponentIdentifier(stringID,ComponentType);
//         Content = new MovementContent();
//     }
//     
//     public eWorkoutComponents ComponentType => eWorkoutComponents.Movement;
//     
//     public ComponentDescriptor Descriptor { get; set; }
//     public WorkoutComponentIdentifier Identifier { get; set; }
//     public MediaInfos Medias { get; set; }
//
//     public MovementContent Content { get; set; }
//
//     public void AddEquipmentList(EquipmentList equipments) =>
//         Content.AddEquipmentList(equipments);
//     
//     public void AddEquipment(Equipment.Equipment equipment) => 
//         Content.AddEquipment(equipment);
//     
//     public void AddVariantOf(Movement movement) =>
//         Content.AddVariantOf(movement);
//     public void AddMainMuscles(MuscleList muscleList) => 
//         Content.AddMainMuscles(muscleList);
//
//     public void AddSecondaryMuscles(MuscleList muscleList) => 
//         Content.AddSecondaryMuscles(muscleList);
//
//     public void AddMusclesGroup(MusclesGroup musclesGroup) => 
//         Content.AddMuscleGroups(musclesGroup);
//
//     public void AddMovementCategory(MovementCategory.MovementCategory movementCategory) => 
//         Content.AddMovementCategory(movementCategory);
//
//
//     public void AddWorkloadCalculationProfile(WorkloadCalculationProfile profile) =>
//         Content.AddWorkloadProfile(profile);
//
//     public IWorkoutComponent Copy()
//     {
//         throw new System.NotImplementedException();
//     }
//     
//     public override string ToString()
//     {
//         return $"Movement: \"{Descriptor?.Name ?? "Unnamed"}\", ID: [{Identifier}], Content Summary:\n[{Content?.ToString()}]";
//     }
//
//  
//}