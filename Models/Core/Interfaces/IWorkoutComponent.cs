using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using GainsLab.Models.Core.WorkoutComponents;

namespace GainsLab.Models.Core;


//basic bloc of a item part of a workout (i.e. equipment, muscles, movements etc)
//could be also implementing IExecutableComponent 
[JsonPolymorphic(TypeDiscriminatorPropertyName = "$type")]
[JsonDerivedType(typeof(Equipment), "equipment")]
[JsonDerivedType(typeof(Muscle), "muscle")]
//todo add all type
public interface IWorkoutComponent
{
  
    
    string Name => Descriptor.Name;
    
    eWorkoutComponents ComponentType { get;}
    ComponentDescriptor Descriptor { get; set; }
    Identifier Identifier { get; set; }
    
    IWorkoutComponent Copy();
    

}

public interface ICompositeWorkoutComponent<TComponentContent> : IWorkoutComponent where TComponentContent : IComponentContent
{
    TComponentContent Content { get; set; }
  
}