using System.Threading.Tasks;

namespace GainsLab.Models.Core.Interfaces;

public interface IComponentReference
{
    bool IsComponentResolved { get; }
    Identifier Identifier { get; }
    IWorkoutComponent? AsWorkoutComponent();
 
}