using System;
using System.Collections.Generic;
using System.Linq;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Utilities;

namespace GainsLab.Models.Core;

// public class ComponentContent : IComponentContent
// {
//     
//     public ComponentContent(){}
//
//     public ComponentContent(eWorkoutComponents type, ComponentReference<IWorkoutComponent>? component)
//     {
//         if(component == null) return;
//         
//         Add(type, component);
//     }
//
//     public Dictionary<eWorkoutComponents, IComponentList> Items { get; set; } = new();
//
//     public void Add(eWorkoutComponents type,  ComponentReference<IWorkoutComponent> component)
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
//         Items[type] = new ComponentLists< IWorkoutComponent>(component);
//     }
//     
//     public void Add(eWorkoutComponents type, ComponentLists<IWorkoutComponent> components)
//         => AddMany(type, components.Items);
//     
//     public void Add(eWorkoutComponents type, IWorkoutComponent component) 
//         => Add(type,ComponentReference<IWorkoutComponent>.FromComponent(component));
//     
//     public void Add(eWorkoutComponents type, IIdentifier identifier)
//         => Add(type,ComponentReference<IWorkoutComponent>.FromIdentifier(identifier));
//
//     public void AddMany(eWorkoutComponents type, List<ComponentReference<IWorkoutComponent>> components)
//         =>  components.ForEach(comp => Add(type, comp)); 
//
//     public void AddMany(eWorkoutComponents type, List<IWorkoutComponent> components) 
//         => components.ForEach(comp => Add(type, comp)); 
//
//     public void AddMany(eWorkoutComponents type, List<IIdentifier> identifiers)
//         =>  identifiers.ForEach(id => Add(type, id));
//
//
//     public List<ComponentReference<T>> GetReferencesOfType<T>() where T : IWorkoutComponent
//     {
//         var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
//         Console.WriteLine($"[GetReferencesOfType] Requested type: {typeof(T).Name}, resolved eWorkoutComponent: {type}");
//
//         if (Items.TryGetValue(type, out var list))
//         {
//             Console.WriteLine($"[GetReferencesOfType] Found list for type {type}. Total references: {list.References.Count()}");
//
//             var filtered = list.ToTypedReferences<T>();
//             
//           //  Console.WriteLine($"[GetReferencesOfType] Successfully filtered {filtered.Count()} references of type {typeof(T).Name}");
//
//             return filtered.ToList();
//         }
//
//         Console.WriteLine($"[GetReferencesOfType] No entry found for type {type}");
//         return new List<ComponentReference<T>>();
//     }
//
//     public void SetComponentInReference<T>(T component) where T : IWorkoutComponent
//     {
//         ComponentReference<T>? compoRef = GetComponentReferenceWithId<T>(component.Identifier);
//         if (compoRef == null)
//         {
//             Console.WriteLine($"No component reference found with id {component.Identifier.ToString()}");
//             return;
//         }
//
//         compoRef.Component = component;
//     }
//
//     private ComponentReference<T>? GetComponentReferenceWithId<T>(IIdentifier identifier) where T : IWorkoutComponent
//     {
//         var refs = GetReferencesOfType<T>();
//         return refs.FirstOrDefault(it => it.Identifier.Equals(identifier));
//     }
//     
//     public List<ComponentReference<T>> GetUnResolvedReferencesOfType<T>() where T : IWorkoutComponent
//     {
//         var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
//     
//         if (Items.TryGetValue(type, out var list))
//         {
//             return list.ToTypedReferences<T>()
//                 .Where(r => !r.IsComponentResolved)
//                 .Select(r => new ComponentReference<T>
//                 {
//                     Identifier = r.Identifier
//                 })
//                 .ToList();
//         }
//
//         return new List<ComponentReference<T>>();
//     }
//     
//     
//     public List<T> GetResolvedComponentsOfType<T>() where T : IWorkoutComponent
//     {
//         var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
//         if (Items.TryGetValue(type, out var list))
//         {
//             
//             return list.ToTypedReferences<T>()
//                 .Where(it=>it.IsComponentResolved)
//                 .Select(it=> it.Component)
//                 .Where(it => it != null)
//                 .OfType<T>()
//                 .ToList();
//         }
//
//         return new List<T>();
//     }
//
//    
//
//     public List<IIdentifier> GetIdsOfType<T>() where T : IWorkoutComponent
//     {
//         var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
//         if (Items.TryGetValue(type, out var list))
//         {
//             
//             return list.References
//                 .Select(it=> (IIdentifier)it.Identifier)
//                 .ToList();
//         }
//
//         return new ();
//     }
//     
//
//     public bool Contains(IIdentifier id)
//         => Items.Values
//             .SelectMany(it => it.References)
//             .Any(it => it.Identifier.Equals(id));
//
//     
//     public Dictionary<eWorkoutComponents, ComponentLists<IWorkoutComponent>> GetAllUnresolvedReferences()
//     {
//         if (Items.Count == 0) return new();
//
//         Dictionary<eWorkoutComponents, ComponentLists<IWorkoutComponent>> dictionary = new();
//         
//         foreach (var kvp in Items)
//         {
//             var type = kvp.Key;
//             var list = kvp.Value;
//             var unresolved = list.GetUnresolvedReferences().ToList();
//             if (unresolved.Count == 0) continue;
//             
//             dictionary[type] = new ComponentLists<IWorkoutComponent>(unresolved);
//         }
//
//         return dictionary;
//     }
//
//     public List<T> GetUnresolvedReferencesOfType<T>() where T : IWorkoutComponent
//     {
//       
//         var type = CoreUtilities.GetComponentTypeFromGeneric<T>();
//         if (Items.TryGetValue(type, out var list))
//         {
//             
//             return list.ToTypedReferences<T>().
//                 Where(it=>!it.IsComponentResolved)
//                 .Select(it=> it.Component)
//                 .Where(it => it != null)
//                 .OfType<T>()
//                 .ToList();
//         }
//
//         return new List<T>();
//         
//     }
//     
//     public override string ToString()
//     {
//         if (Items.Count == 0) return "ComponentContent: (empty)";
//         
//         var summary = Items.Select(kvp =>
//         {
//             var type = kvp.Key;
//             var list = kvp.Value;
//             var resolved = list.References.Count(r => r.IsComponentResolved);
//             return $"{type} -> Count: {list.References.Count()}, Resolved: {resolved}";
//         });
//
//         return "ComponentContent:\n" + string.Join("\n", summary);
//     }
//     
//     
// }