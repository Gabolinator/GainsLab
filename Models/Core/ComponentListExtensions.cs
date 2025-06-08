using System;
using System.Collections.Generic;
using System.Linq;

namespace GainsLab.Models.Core;

public static class ComponentListExtensions
{
    
    
    
    
    public static ComponentLists<T> GetDistinct<T>(this IEnumerable<ComponentReference<T>> componentReferences) where T : IWorkoutComponent
    {
       return new ComponentLists<T>(componentReferences.Distinct().ToList());
    }
    
    public static ComponentLists<T> GetDistinct<T>(this ComponentLists<T> componentList) where T : IWorkoutComponent
    {
       return componentList.References.ToTypedReferences<T>().GetDistinct();
    }

    
    
    public static IEnumerable<ComponentReference<T>> ToTypedReferences<T>(
        this IEnumerable<ComponentReference<IWorkoutComponent>> list) where T : IWorkoutComponent
    {
        var casted = list
            .Select(r =>
            {
                if (r.Component is T component)
                {
                    return new ComponentReference<T>
                    {
                        Identifier = r.Identifier,
                        Component = component
                    };
                }

                // Log if unresolved or wrong type
                Console.WriteLine(
                    $"[GetReferencesOfType] Skipped unresolved or mismatched component. ID: {r.Identifier?.ID}, Component null: {r.Component == null}");
                return null;
            })
            .Where(x => x != null);
            

        Console.WriteLine($"[GetReferencesOfType] Successfully created {casted.Count()} strongly typed references of type {typeof(T).Name}");

        return casted;
    }
    
    public static IEnumerable<ComponentReference<T>> ToTypedReferences<T>(
        this IComponentList list) where T : IWorkoutComponent
    {
        return list.References.ToTypedReferences<T>();
    }
    
    
    
}