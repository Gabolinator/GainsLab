


using System.Collections.Generic;


namespace GainsLab.Models.Core.Interfaces;

public interface IComponentContent
{
    Dictionary<eWorkoutComponents, IComponentList> Items { get; set; }

    public void Add(eWorkoutComponents type,  ComponentLists<IWorkoutComponent> components);
    public void Add(eWorkoutComponents type,  ComponentReference<IWorkoutComponent> component);
    
    public void Add(eWorkoutComponents type,  IWorkoutComponent component);
    
    public void Add(eWorkoutComponents type,  IIdentifier identifier);
    
    public void AddMany(eWorkoutComponents type,  List<ComponentReference<IWorkoutComponent>> components);
    
    public void AddMany(eWorkoutComponents type,  List<IWorkoutComponent> components);
    
    public void AddMany(eWorkoutComponents type,  List<IIdentifier> identifiers);
    
    public List<ComponentReference<T>> GetReferencesOfType<T>() where T : IWorkoutComponent;

    public List<IIdentifier> GetIdsOfType<T>() where T : IWorkoutComponent;
    public List<T> GetResolvedComponentsOfType<T>() where T : IWorkoutComponent;

    public Dictionary<eWorkoutComponents, ComponentLists<IWorkoutComponent>> GetAllUnresolvedReferences();
    
    public List<T> GetUnresolvedReferencesOfType<T>() where T : IWorkoutComponent;

    public bool Contains(IIdentifier id);






}