using GainsLab.Core.Models.Core;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.WorkoutComponents.MovementCategory;

// public class MovementCategory : IWorkoutComponent
// {
//     public int Id
//     {
//         get => Identifier.DbID ?? -1;
//
//         set => Identifier.DbID = value;
//     }
//     
//     public MovementCategory()
//     {
//         Identifier = new EmptyWorkoutComponentIdentifier();
//         Descriptor = new ComponentDescriptor();
//     }
//
//     public MovementCategory(ComponentDescriptor descriptor, WorkoutComponentIdentifier identifier, eMovementCategories baseCategory)
//     {
//         Descriptor = descriptor;
//         Identifier = identifier;
//         BaseCategory = baseCategory;
//     }
//
//     public MovementCategory(string name, string stringID, eMovementCategories baseCategory)
//     {
//         Descriptor = new ComponentDescriptor(name, ComponentType);
//         Identifier = new WorkoutComponentIdentifier(stringID, ComponentType);
//         BaseCategory = baseCategory;
//     }
//     
//     
//
//     public eWorkoutComponents ComponentType => eWorkoutComponents.MovementCategory;
//
//     public eMovementCategories BaseCategory { get; set; } = eMovementCategories.undefined;
//     public ComponentDescriptor Descriptor { get; set; }
//     public WorkoutComponentIdentifier Identifier { get; set; }
//     public MediaInfos Medias { get; set; }
//
//     public IWorkoutComponent Copy()
//     {
//         throw new System.NotImplementedException();
//     }
//     
//     public override string ToString()
//     {
//         return $"MovementCategory: \"{Descriptor?.Name ?? "Unnamed"}\", Base={BaseCategory}, ID={Identifier}";
//     }
// }