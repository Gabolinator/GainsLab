namespace GainsLab.Models.Core;


//a workout component that can be executed - a movement, an exercice, a workout etc 
//NOT a muscles 
public interface IExecutableComponent
{
    IExecutionContent ExecutionContent { get; set; }
}