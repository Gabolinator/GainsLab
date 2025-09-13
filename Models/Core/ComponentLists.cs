
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json.Serialization;
using GainsLab.Models.App;
using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Logging;


namespace GainsLab.Models.Core;

public class ComponentLists<TComponent> : IComponentList
    where TComponent : IWorkoutComponent
{
 
    ILogger Logger => ServiceLocator.GetService<ILogger>();
   // logger?.Log("ComponentReference - FromComponent", $" {typeof(TComponent).Name} ({component.Name}) to ComponentReference<{typeof(TComponent).Name}>");

    
    
    [JsonIgnore]
    public List<ComponentReference<TComponent>> Items { get; set; } = new();
    
    public virtual eWorkoutComponents ComponentsType { get; set; } = eWorkoutComponents.unidentified;

    public ComponentLists() { }

    public ComponentLists(ComponentReference<TComponent> component)
    {
        ComponentsType = component.ComponentType;
        Items.Add(component);
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
            Logger?.LogWarning("ComponentList",$"Type mismatch: expected {ComponentsType}, got {type}");
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
            Logger?.LogWarning("ComponentList",$"Failed to cast to correct type {typeof(TComponent).Name}");
        }
    }
    
    public void AddComponent(ComponentReference<TComponent> reference)
    {
        if (IsTypeMismatch(reference.ComponentType)) return;
        
        Items.Add(reference);
    }

    public void AddComponent(TComponent component)
    {
        if (IsTypeMismatch(component.ComponentType)) return;
        var reference = ComponentReference<TComponent>.FromComponent(component);
        
        Items.Add(reference);
      
    }

    public void AddComponent(IIdentifier id)
    {
        if (IsTypeMismatch(id.ComponentType)) return;
        Items.Add(ComponentReference<TComponent>.FromIdentifier(id));
    }

    public void AddComponents(IEnumerable<TComponent> components)
    {
        Logger?.Log("ComponentList",$"Adding { components.Count()} components");
        
        foreach (var comp in components) AddComponent(comp);
    }

    public void AddComponents(IEnumerable<IIdentifier> ids)
    {  
        Logger?.Log("ComponentList",$"Adding { ids.Count()} components ids");
        foreach (var id in ids) AddComponent(id);
    }

    public void AddComponents(IEnumerable<ComponentReference<TComponent>> refs)
    {
        Logger?.Log("ComponentList",$"Adding { refs.Count()} components references of type {(refs.Any() ? refs.ElementAt(0).ComponentType : "no element in list")}");
        foreach (var r in refs) AddComponent(r);
    }

    public void AddComponents(ComponentLists<TComponent> otherList)
    {
        Logger?.Log("ComponentList",$"Adding {otherList.Items.Count} components of type {(otherList.Items.Any() ? otherList.Items[0].ComponentType : "no element in list")} from component list");
        foreach (var r in otherList.Items) AddComponent(r);
    }

    public IEnumerable<ComponentReference<IWorkoutComponent>> GetUnresolvedReferences() =>
         References.Where(r => !r.IsComponentResolved);
    
    
    
    public List<TComponent> GetResolvedComponents() =>
        this.Items.Where(r => r.IsComponentResolved).Select(r => r.Component!).ToList();

    public List<ComponentReference<TComponent>> GetResolvedReferences() =>
        this.Items.Where(r => r.IsComponentResolved).ToList();

    public IEnumerable<ComponentReference<IWorkoutComponent>> References =>
        this.Items.Select(r => new ComponentReference<IWorkoutComponent>
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

        foreach (var item in a.Items)
        {
            if (!b.Items.Any(x => x.Identifier.Equals(item.Identifier)))
            {
                result.Items.Add(item);
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

        foreach (var reference in this.Items)
        {
            var referenceCopy = new ComponentReference<TComponent>
            {
                Identifier = reference.Identifier, // Assuming identifiers are immutable or copied properly
                Component = reference.Component // This assumes components themselves are shared or immutable
            };

            // Optionally deep copy the component if needed:
            // referenceCopy.Component = reference.Component?.Copy() as TComponent;

            copy.Items.Add(referenceCopy);
        }

        return copy;
    }
    

    
    
    
    public override string ToString()
    {
        
        var resolvedCount = this.Items.Count(c => c.IsComponentResolved);
        var content = this.Items.Count(c => c.IsComponentResolved) > 0 ? string.Join(", ", this.Items.Where(c => c.IsComponentResolved).Select(it => it.Component.Name)): "none";
        return $"ComponentList<{typeof(TComponent).Name}> of type {ComponentsType}, Count: {Items.Count}, Resolved: {resolvedCount} - Contents : {content}";
    }
    
}

