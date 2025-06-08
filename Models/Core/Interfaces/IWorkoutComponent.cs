using System.Runtime.CompilerServices;

namespace GainsLab.Models.Core;


//basic bloc of a item part of a workout (i.e. equipment, muscles, movements etc)
//could be also implementing IExecutableComponent 

public interface IWorkoutComponent
{
    string Name => Descriptor.Name;
    
    eWorkoutComponents ComponentType { get;}
    IComponentDescriptor Descriptor { get; set; }
    IIdentifier Identifier { get; set; }
    
    IWorkoutComponent Copy();
    

}

public interface ICompositeWorkoutComponent<TComponentContent> : IWorkoutComponent where TComponentContent : IComponentContent
{
    TComponentContent Content { get; set; }
  
}