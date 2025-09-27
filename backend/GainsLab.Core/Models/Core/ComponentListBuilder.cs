using System;
using System.Collections.Generic;

namespace GainsLab.Models.Core;

// public class ComponentListBuilder<TComponent> where TComponent : IWorkoutComponent
// {
//     private readonly ComponentLists<TComponent> _list;
//
//     public ComponentListBuilder()
//     {
//         _list = new ComponentLists<TComponent>();
//     }
//     
//     private bool IsTypeMismatch(eWorkoutComponents type)
//     {
//         if (_list.ComponentsType == eWorkoutComponents.unidentified)
//         {
//             _list.ComponentsType = type;
//             return false;
//         }
//
//         return _list.ComponentsType != type;
//     }
//
//     public ComponentListBuilder<TComponent> WithComponentReference(ComponentReference<TComponent> component)
//     {
//         if (IsTypeMismatch(component.ComponentType))
//         {
//             Console.WriteLine($"[TypeMismatch] Tried to add {component.ComponentType} to {_list.ComponentsType} list.");
//             return this;
//         }
//
//         _list.Add(component);
//         return this;
//     }
//     
//     public ComponentListBuilder<TComponent> WithComponentReferences(ComponentLists<TComponent> components)
//     {
//         if (IsTypeMismatch(components.ComponentsType))
//         {
//             Console.WriteLine($"[TypeMismatch] Tried to add {components.ComponentsType} to {_list.ComponentsType} list.");
//             return this;
//         }
//
//         _list.AddRange(components);
//         return this;
//     }
//     
//     public ComponentListBuilder<TComponent> WithComponentReferences(List<ComponentReference<TComponent>> components)
//     {
//         if (components.Count == 0) return this;
//         
//         if (IsTypeMismatch(components[0].ComponentType))
//         {
//             Console.WriteLine($"[TypeMismatch] Tried to add {components[0].ComponentType} to {_list.ComponentsType} list.");
//             return this;
//         }
//
//         _list.AddRange(components);
//         return this;
//     }
//     
//
//     public ComponentListBuilder<TComponent> WithComponent(TComponent component)
//         => WithComponentReference(ComponentReference<TComponent>.FromComponent(component));
//
//     public ComponentListBuilder<TComponent> WithIdentifier(IIdentifier identifier)
//         => WithComponentReference(ComponentReference<TComponent>.FromIdentifier(identifier));
//
//     public ComponentListBuilder<TComponent> WithComponents(List<TComponent> components)
//     {
//         foreach (var comp in components)
//             WithComponent(comp);
//         return this;
//     }
//
//     public ComponentListBuilder<TComponent> WithIdentifiers(List<IIdentifier> identifiers)
//     {
//         foreach (var id in identifiers)
//             WithIdentifier(id);
//         return this;
//     }
//
//     public ComponentLists<TComponent> Build() => _list;
// }