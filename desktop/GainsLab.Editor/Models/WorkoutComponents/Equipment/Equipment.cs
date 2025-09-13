using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.WorkoutComponents.Equipment;

public class Equipment : IWorkoutComponent
{
    
    public Equipment()
    {
        Identifier = new EmptyIdentifier();
        Descriptor = new ComponentDescriptor();
    }

    public Equipment(ComponentDescriptor descriptor, Identifier identifier)
    {
        Descriptor = descriptor;
        Identifier = identifier;
    }
    
    public Equipment(string name, string stringID)
    {
        Descriptor = new ComponentDescriptor(name,ComponentType);
        Identifier = new Identifier(stringID,ComponentType);
    }
    

    public eWorkoutComponents ComponentType => eWorkoutComponents.Equipment;
    
    public ComponentDescriptor Descriptor { get; set; } 
    public Identifier Identifier { get; set; }
    
    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public string UID
    {
        get => Identifier.UID ?? $"Unknown {ComponentType} UID";

        set => Identifier.UID = value;
    }
    
    
    public IWorkoutComponent Copy()
    {
        return new Equipment(Descriptor, Identifier);
    }


    public override string ToString()
    {
        return $"Equipment: \"{Descriptor?.Name ?? "Unnamed"}\", ID: [{Identifier}], Type: {ComponentType}";
    }
}