using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Results;
﻿using GainsLab.Contracts.Dtos.GetDto;

namespace GainsLab.Infrastructure.Caching.QueryCache;

public sealed class MovementCategoryQueryCache :ICache
{
    private readonly object _lock = new();
    private Task<Result<IReadOnlyList<MovementCategoryGetDTO>>>? _task;

    public Task<Result<IReadOnlyList<MovementCategoryGetDTO>>> GetAllAsync(
        Func<Task<Result<IReadOnlyList<MovementCategoryGetDTO>>>> factory)
    {
        lock (_lock)
        {
            return _task ??= factory();
        }
    }

    public bool TryGetCompleted(out Result<IReadOnlyList<MovementCategoryGetDTO>> result)
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
