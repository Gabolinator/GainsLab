using System;
using System.ComponentModel.DataAnnotations.Schema;

using GainsLab.Models.Core.Interfaces;


namespace GainsLab.Models.Core;

// OUTDATED: Legacy ComponentReference implementation retained for later refactor.

// [NotMapped] 
// public class ComponentReference<TComponent> : IComponentReference,  IEquatable<ComponentReference<TComponent>> where TComponent : IWorkoutComponent 
// {
//     
//    
//     
//     public int Id { get =>Identifier.DbID ?? -1; set => Identifier.DbID = value; } 
//     
//     public WorkoutComponentIdentifier Identifier { get; set; } = new EmptyWorkoutComponentIdentifier();
//    
//     [NotMapped]
//     public TComponent? Component { get; set; }
//     public eWorkoutComponents ComponentType => Identifier.ComponentType;
//     public bool IsComponentResolved => Component != null;
//
//    
//
//     public void AssignComponent(TComponent? component)
//     {
//         Component = component;
//     }
//     
//     
//     public IWorkoutComponent? AsWorkoutComponent()
//     {
//         return Component ==null ? null: Component as IWorkoutComponent;
//     }
//     
//     
//     public bool Equals(ComponentReference<TComponent>? other)
//     {
//         if (other is null) return false;
//         return Identifier.Equals(other.Identifier);
//     }
//
//     public override bool Equals(object? obj)
//     {
//         return Equals(obj as ComponentReference<TComponent>);
//     }
//
//     public override int GetHashCode()
//     {
//         return Identifier.GetHashCode();
//     }
//
//     public override string ToString()
//     {
//         var resolvedStatus = IsComponentResolved ? "Resolved" : "Unresolved";
//         var componentTypeName = typeof(TComponent).Name;
//         var id = Identifier?.DbID ?? -1;
//         var uid =  Identifier?.Slug ?? "null";
//         var name = IsComponentResolved ? Component?.Descriptor?.Name ?? "Unnamed" : "Not Loaded";
//
//         return $"[{resolvedStatus}] ComponentReference<{componentTypeName}>: ID = {id}, UID = {uid} ,Name = {name}";
//     }
//
//     public static ComponentReference<TComponent> FromComponent(TComponent component)
//     {
//
//         ILogger logger = CoreUtilities.Logger;
//         logger?.Log("ComponentReference - FromComponent", $" {typeof(TComponent).Name} ({component.Name}) to ComponentReference<{typeof(TComponent).Name}>");
//         
//         return new ComponentReference<TComponent>
//         {
//             Identifier = (WorkoutComponentIdentifier)component.Identifier,
//             Component = component
//         };
//     }
//     
//     public static ComponentReference<TComponent> FromIdentifier(IIdentifier identifier)
//     {
//         return new ComponentReference<TComponent>
//         {
//             Identifier = (WorkoutComponentIdentifier)identifier,
//         };
//     }
//
//     
//    
// }
