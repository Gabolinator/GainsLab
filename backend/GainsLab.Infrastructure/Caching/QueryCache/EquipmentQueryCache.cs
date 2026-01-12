
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Infrastructure.Caching.QueryCache;

public sealed class EquipmentQueryCache
{
    private readonly object _lock = new();
    private Task<Result<IReadOnlyList<EquipmentGetDTO>>>? _task;

    public Task<Result<IReadOnlyList<EquipmentGetDTO>>>  GetAllAsync(Func<Task<Result<IReadOnlyList<EquipmentGetDTO>>>> factory)
    {
        lock (_lock)
            return _task ??= factory();
    }

    public bool TryGetCompleted(out Result<IReadOnlyList<EquipmentGetDTO>> result)
    {
        lock (_lock)
        {
            if (_task is { IsCompletedSuccessfully: true })
            {
                result = _task.Result;
                return true;
            }
        }

        result = default!;
        return false;
    }

    public void Invalidate()
    {
        lock (_lock) _task = null;
    }
}

