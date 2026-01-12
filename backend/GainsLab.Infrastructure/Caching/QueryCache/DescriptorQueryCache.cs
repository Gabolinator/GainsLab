using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Infrastructure.Caching.QueryCache;

public sealed class DescriptorQueryCache
{
    private readonly object _lock = new();
    private Task<Result<IReadOnlyList<DescriptorGetDTO>>>? _task;

    public Task<Result<IReadOnlyList<DescriptorGetDTO>>> GetAllAsync(
        Func<Task<Result<IReadOnlyList<DescriptorGetDTO>>>> factory)
    {
        lock (_lock)
        {
            return _task ??= factory();
        }
    }

    public bool TryGetCompleted(out Result<IReadOnlyList<DescriptorGetDTO>> result)
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
        lock (_lock)
        {
            _task = null;
        }
    }
}
