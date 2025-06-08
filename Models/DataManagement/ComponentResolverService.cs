using System.Linq;
using System.Threading.Tasks;
using GainsLab.Models.Core;
using GainsLab.Models.Logging;

namespace GainsLab.Models.DataManagement;

public class ComponentResolverService
{

    private readonly IDataManager _dataManager;
    private readonly IWorkoutLogger _logger;

    public ComponentResolverService(IWorkoutLogger logger, IDataManager dataManager)
    {
        _logger = logger;
        _dataManager = dataManager;
    }
    
    public async Task ResolveComponentsAsync<TComponent>(ComponentLists<TComponent> list)
        where TComponent : IWorkoutComponent
    {
        var unresolved = list
            .Where(r => !r.IsComponentResolved)
            .Select(r => r.Identifier)
            .ToList();

        if (!unresolved.Any()) return;

        var resolved = await _dataManager.ResolveComponentsAsync<TComponent>(unresolved);

        foreach (var comp in resolved)
        {
            var refToUpdate = list.FirstOrDefault(r => r.Identifier.Equals(comp.Identifier));
            if (refToUpdate != null)
                refToUpdate.Component = comp;
        }
    }
}