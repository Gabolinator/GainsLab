using GainsLab.Models.Core;

namespace GainsLab.Models.WorkoutComponents.MovementCategory;

public class MovementCategory : IWorkoutComponent
{

    public MovementCategory(IComponentDescriptor descriptor, IIdentifier identifier, eMovementCategories baseCategory)
    {
        Descriptor = descriptor;
        Identifier = identifier;
        BaseCategory = baseCategory;
    }

    public MovementCategory(string name, string stringID, eMovementCategories baseCategory)
    {
        Descriptor = new ComponentDescriptor(name, ComponentType);
        Identifier = new Identifier(stringID, ComponentType);
        BaseCategory = baseCategory;
    }
    
    

    public eWorkoutComponents ComponentType => eWorkoutComponents.MovementCategory;

    public eMovementCategories BaseCategory { get; set; } = eMovementCategories.undefined;
    public IComponentDescriptor Descriptor { get; set; }
    public IIdentifier Identifier { get; set; }
    
    public IWorkoutComponent Copy()
    {
        throw new System.NotImplementedException();
    }
    
    public override string ToString()
    {
        return $"MovementCategory: \"{Descriptor?.Name ?? "Unnamed"}\", Base={BaseCategory}, ID={Identifier}";
    }
}