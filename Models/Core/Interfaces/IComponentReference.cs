using System.Threading.Tasks;

namespace GainsLab.Models.Core;

public interface IComponentReference
{
    bool IsComponentResolved { get; }
    Identifier Identifier { get; }
    IWorkoutComponent? AsWorkoutComponent();
 
}