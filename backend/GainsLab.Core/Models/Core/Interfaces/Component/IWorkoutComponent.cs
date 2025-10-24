using System;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.WorkoutComponents.Equipment;
using GainsLab.Models.WorkoutComponents.Muscle;

namespace GainsLab.Models.Core.Interfaces;


//basic bloc of a item part of a workout (i.e. equipment, muscles, movements etc)
//could be also implementing IExecutableComponent 
// OUTDATED: Legacy IWorkoutComponent contract retained for later refactor.
// [JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
// [JsonDerivedType(typeof(Equipment), "equipment")]
// [JsonDerivedType(typeof(Muscle), "muscle")]
// //todo add all type
// public interface IWorkoutComponent
// {
//   
//     
//     string Name => Descriptor.Name;
//     
//     public Guid GUID
//     {
//         get => Identifier.GUID;
//
//         set => Identifier.GUID = value;
//     }
//     
//     //the type of the component - exercise , 
//     eWorkoutComponents ComponentType { get;}
//     ComponentDescriptor Descriptor { get; set; }
//     WorkoutComponentIdentifier Identifier { get; set; }
//
//     
//
//     IWorkoutComponent Copy();
//     
//
// }
//
// public interface ICompositeWorkoutComponent<TComponentContent> : IWorkoutComponent where TComponentContent : IComponentContent
// {
//     TComponentContent Content { get; set; }
//   
// }
