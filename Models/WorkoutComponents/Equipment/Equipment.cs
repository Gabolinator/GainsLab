using GainsLab.Models.Core;

namespace GainsLab.Models.Core.WorkoutComponents;

public class Equipment : IWorkoutComponent
{
    public Equipment(IComponentDescriptor descriptor, IIdentifier identifier)
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
    
    public IComponentDescriptor Descriptor { get; set; } 
    public IIdentifier Identifier { get; set; }
    
    public IWorkoutComponent Copy()
    {
        return new Equipment(Descriptor, Identifier);
    }


    public override string ToString()
    {
        return $"Equipment: \"{Descriptor?.Name ?? "Unnamed"}\", ID: [{Identifier}], Type: {ComponentType}";
    }
}