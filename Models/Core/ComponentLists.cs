
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GainsLab.Models.WorkoutComponents;


namespace GainsLab.Models.Core;

public class ComponentLists<TComponent> : List<ComponentReference<TComponent>>, IComponentList
    where TComponent : IWorkoutComponent
{
    public virtual eWorkoutComponents ComponentsType { get; set; } = eWorkoutComponents.unidentified;

    public ComponentLists() { }

    public ComponentLists(ComponentReference<TComponent> component)
    {
        ComponentsType = component.ComponentType;
        Add(component);
    }
    
    public ComponentLists(List<ComponentReference<TComponent>> components)
    {
        if(components.Count ==0) return;
        
        ComponentsType = components[0].ComponentType;
        AddComponents(components);
    }
    
    
    private bool IsTypeMismatch(eWorkoutComponents type)
    {
        if (ComponentsType == eWorkoutComponents.unidentified)
        {
            ComponentsType = type;
            return false;
        }

        if (ComponentsType != type)
        {
            Console.WriteLine($"Type mismatch: expected {ComponentsType}, got {type}");
            return true;
        }

        return false;
    }


  

    public void AddComponent(ComponentReference<IWorkoutComponent> reference)
    {
        if (reference is ComponentReference<TComponent> concrete)
        {
            AddComponent(concrete);
        }
        else
        {
            Console.WriteLine($"Failed to cast to correct type {typeof(TComponent).Name}");
        }
    }
    
    public void AddComponent(ComponentReference<TComponent> reference)
    {
        if (IsTypeMismatch(reference.ComponentType)) return;
        Add(reference);
    }

    public void AddComponent(TComponent component)
    {
        if (IsTypeMismatch(component.ComponentType)) return;
        Add(ComponentReference<TComponent>.FromComponent(component));
    }

    public void AddComponent(IIdentifier id)
    {
        if (IsTypeMismatch(id.ComponentType)) return;
        Add(ComponentReference<TComponent>.FromIdentifier(id));
    }

    public void AddComponents(IEnumerable<TComponent> components)
    {
        Console.WriteLine($"[ComponentLists] Adding { components.Count()} components");
        foreach (var comp in components) AddComponent(comp);
    }

    public void AddComponents(IEnumerable<IIdentifier> ids)
    {
        Console.WriteLine($"[ComponentLists] Adding { ids.Count()} components ids");
        foreach (var id in ids) AddComponent(id);
    }

    public void AddComponents(IEnumerable<ComponentReference<TComponent>> refs)
    {
        Console.WriteLine($"[ComponentLists] Adding { refs.Count()} components references of type {(refs.Any() ? refs.ElementAt(0).ComponentType : "no element in list")}");
        foreach (var r in refs) AddComponent(r);
    }

    public void AddComponents(ComponentLists<TComponent> otherList)
    {
        Console.WriteLine($"[ComponentLists] Adding {otherList.Count} components of type {(otherList.Any() ? otherList[0].ComponentType : "no element in list")} from component list");
        foreach (var r in otherList) AddComponent(r);
    }

    public IEnumerable<ComponentReference<IWorkoutComponent>> GetUnresolvedReferences() =>
         References.Where(r => !r.IsComponentResolved);
    
    
    
    public List<TComponent> GetResolvedComponents() =>
        this.Where(r => r.IsComponentResolved).Select(r => r.Component!).ToList();

    public List<ComponentReference<TComponent>> GetResolvedReferences() =>
        this.Where(r => r.IsComponentResolved).ToList();

    public IEnumerable<ComponentReference<IWorkoutComponent>> References =>
        this.Select(r => new ComponentReference<IWorkoutComponent>
        {
            Identifier = r.Identifier,
            Component = r.Component
        });


     


    public static ComponentLists<TComponent> operator +(ComponentLists<TComponent> a, ComponentLists<TComponent> b) 
    {
        if (a.ComponentsType != b.ComponentsType)
        {
            throw new InvalidOperationException($"Cannot combine ComponentLists of different types: {a.ComponentsType} and {b.ComponentsType}");
        }

        var result = new ComponentLists<TComponent> { ComponentsType = a.ComponentsType };
        result.AddComponents(a);
        result.AddComponents(b);
        return result;
    }

    
    
    public static ComponentLists<TComponent> operator -(ComponentLists<TComponent> a, ComponentLists<TComponent> b)
    {
        if (a.ComponentsType != b.ComponentsType)
        {
            throw new InvalidOperationException($"Cannot subtract ComponentLists of different types: {a.ComponentsType} and {b.ComponentsType}");
        }

        var result = new ComponentLists<TComponent> { ComponentsType = a.ComponentsType };

        foreach (var item in a)
        {
            if (!b.Any(x => x.Identifier.Equals(item.Identifier)))
            {
                result.Add(item);
            }
        }

        return result;
    }

    public ComponentLists<TComponent> CopyList()
    {
        var copy = new ComponentLists<TComponent>
        {
            ComponentsType = this.ComponentsType
        };

        foreach (var reference in this)
        {
            var referenceCopy = new ComponentReference<TComponent>
            {
                Identifier = reference.Identifier, // Assuming identifiers are immutable or copied properly
                Component = reference.Component // This assumes components themselves are shared or immutable
            };

            // Optionally deep copy the component if needed:
            // referenceCopy.Component = reference.Component?.Copy() as TComponent;

            copy.Add(referenceCopy);
        }

        return copy;
    }
    
    
    public override string ToString()
    {
        
        var resolvedCount = this.Count(c => c.IsComponentResolved);
        var content = this.Count(c => c.IsComponentResolved) > 0 ? string.Join(", ", this.Where(c => c.IsComponentResolved).Select(it => it.Component.Name)): "none";
        return $"ComponentList<{typeof(TComponent).Name}> of type {ComponentsType}, Count: {Count}, Resolved: {resolvedCount} - Contents : {content}";
    }
    
}

