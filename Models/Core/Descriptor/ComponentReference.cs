using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using GainsLab.Models.DataManagement;

namespace GainsLab.Models.Core;

public class ComponentReference<TComponent> : IComponentReference,  IEquatable<ComponentReference<TComponent>> where TComponent : IWorkoutComponent 
{
    
    public Func<IIdentifier, Task<TComponent?>>? ComponentResolver { get; set; }
    public IIdentifier Identifier { get; set; } = new EmptyIdentifier();
    public TComponent? Component { get; set; }
    public eWorkoutComponents ComponentType => Identifier.ComponentType;
    public bool IsComponentResolved => Component != null;

    public async Task<TComponent?> GetOrResolveComponentAsync()
    {
        if (IsComponentResolved) return Component;
        var component = await ComponentResolver!(Identifier);
        AssignComponent(component);
        
        return component;
        
    }

    public void AssignComponent(TComponent? component)
    {
        Component = component;
    }
    
    
    public IWorkoutComponent? AsWorkoutComponent()
    {
        return Component ==null ? null: Component as IWorkoutComponent;
    }
    
    
    public bool Equals(ComponentReference<TComponent>? other)
    {
        if (other is null) return false;
        return Identifier.Equals(other.Identifier);
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as ComponentReference<TComponent>);
    }

    public override int GetHashCode()
    {
        return Identifier.GetHashCode();
    }

    public override string ToString()
    {
        var resolvedStatus = IsComponentResolved ? "Resolved" : "Unresolved";
        var componentTypeName = typeof(TComponent).Name;
        var id = Identifier?.ID ?? -1;
        var uid =  Identifier?.UID ?? "null";
        var name = IsComponentResolved ? Component?.Descriptor?.Name ?? "Unnamed" : "Not Loaded";

        return $"[{resolvedStatus}] ComponentReference<{componentTypeName}>: ID = {id}, UID = {uid} ,Name = {name}";
    }

    public static ComponentReference<TComponent> FromComponent(TComponent component)
    {
        Console.WriteLine($"[FromComponent]   {typeof(TComponent).Name} ({component.Name}) to ComponentReference<{typeof(TComponent).Name}>");
        return new ComponentReference<TComponent>
        {
            Identifier = component.Identifier,
            Component = component
        };
    }
    
    public static ComponentReference<TComponent> FromIdentifier(IIdentifier identifier)
    {
        return new ComponentReference<TComponent>
        {
            Identifier = identifier,
        };
    }

    
   
}