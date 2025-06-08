using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.Caching.Interface;

public interface IComponentCache<T> where T : IWorkoutComponent
{
    bool TryGet(IIdentifier id, out T? component);
    void Store(IIdentifier id, T component);
    void Remove(IIdentifier id);
    void Clear();
}