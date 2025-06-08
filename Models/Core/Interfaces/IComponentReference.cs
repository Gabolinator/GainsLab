using System.Threading.Tasks;

namespace GainsLab.Models.Core;

public interface IComponentReference
{
    bool IsComponentResolved { get; }
    IIdentifier Identifier { get; }
    IWorkoutComponent? AsWorkoutComponent();
 
}