using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Utilities;

namespace GainsLab.Models.Core;

// public class IdentifierContent : IIdentifierContent
// {
//  
//     public IdentifierContent(){}
//
//     public IdentifierContent(eWorkoutComponents type, IWorkoutComponent? component)
//     {
//         if(component == null) return;
//         
//         Add(type, component.Identifier);
//     }
//
//
//     public Dictionary<eWorkoutComponents, ComponentsIdentifierList> Items { get; set; }
//
//     public void Add(eWorkoutComponents type, IIdentifier component)
//     {
//         if (Items.TryGetValue(type, out var componentLists))
//         {
//             if (componentLists.ComponentsType == component.ComponentType)
//             {
//                 componentLists.AddComponent(component);
//                 return;
//             }
//         }
//
//         Items[type] = new ComponentsIdentifierList(component);
//     }
//
//     public List<IIdentifier> GetComponentsOfType<T>() where T : IWorkoutComponent
//     {
//         var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
//         if (Items.TryGetValue(type, out var list))
//         {
//             return list.ToList();
//         }
//
//         return new List<IIdentifier>();
//     }
// }