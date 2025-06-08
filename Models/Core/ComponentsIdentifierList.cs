using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core.WorkoutComponents;

namespace GainsLab.Models.Core;

// public class ComponentsIdentifierList : List<IIdentifier> //where TComponentList : List<TComponent> where TComponent : IWorkoutComponent
// {
//     public  ComponentsIdentifierList() { }
//
//     public  ComponentsIdentifierList(IIdentifier component)
//     {
//         ComponentsType = component.ComponentType;
//         AddComponent(component);
//     }
//     
//     public  ComponentsIdentifierList( IWorkoutComponent component)
//     {
//         ComponentsType = component.ComponentType;
//         AddComponent(component);
//     }
//
//     public  ComponentsIdentifierList( ComponentsIdentifierList component)
//     {
//         ComponentsType = component.ComponentsType;
//         AddComponents(component);
//     }
//
//     public void AddComponent( IWorkoutComponent component)
//     {
//         if (ComponentsType != component.ComponentType)
//         {
//             Console.WriteLine($"Mismatched component type: tried to add {component.ComponentType} to {ComponentsType} list.");
//             return;
//         }
//
//         Add(component.Identifier);
//     }
//     
//     public void AddComponent(IIdentifier component)
//     {
//         if (ComponentsType != component.ComponentType)
//         {
//             Console.WriteLine($"Mismatched component type: tried to add {component.ComponentType} to {ComponentsType} list.");
//             return;
//         }
//
//         Add(component);
//     }
//
//     public void AddComponents( ComponentsIdentifierList componentLists)
//     {
//         if (componentLists == null || componentLists.Count == 0) return;
//
//         if (ComponentsType != componentLists.ComponentsType)
//         {
//             Console.WriteLine($"Mismatched component list type: tried to add {componentLists.ComponentsType} to {ComponentsType} list.");
//             return;
//         }
//
//         AddRange(componentLists);
//     }
//
//     public void AddComponents(ComponentLists<IWorkoutComponent> components)
//     {
//         List<IIdentifier> list = components.Select(it => it.Identifier).ToList();
//         AddComponents(list);
//     }
//
//     public void AddComponents(List< IWorkoutComponent> components)
//     {
//         if (components == null || components.Count == 0) return;
//
//         if (ComponentsType != components[0].ComponentType)
//         {
//             Console.WriteLine($"Mismatched list item type: tried to add {components[0].ComponentType} to {ComponentsType} list.");
//             return;
//         }
//
//         AddRange(components.Select(it=>it.Identifier));
//     }
//     
//     public void AddComponents(List<IIdentifier> components)
//     {
//         if (components == null || components.Count == 0) return;
//
//         if (ComponentsType != components[0].ComponentType)
//         {
//             Console.WriteLine($"Mismatched list item type: tried to add {components[0].ComponentType} to {ComponentsType} list.");
//             return;
//         }
//
//         AddRange(components);
//     }
//
//     public virtual eWorkoutComponents ComponentsType { get; protected set; } = eWorkoutComponents.unidentified;
//
//  
//
//
// }