using GainsLab.Application.DomainMappers;
﻿using GainsLab.Application.Interfaces.DataManagement.Gateway;
﻿using GainsLab.Application.Results;
﻿using GainsLab.Contracts.Dtos.GetDto;
﻿using GainsLab.Infrastructure.Caching.QueryCache;

namespace GainsLab.Infrastructure.Caching.Registry;

public sealed class MuscleRegistry
{
    private readonly IMuscleGateway _gateway;
    private readonly MuscleQueryCache _cache;
    

    public Dictionary<Guid, MuscleGetDTO> Muscles { get; private set; } = new();

    public MuscleRegistry(IMuscleGateway gateway, MuscleQueryCache cache)
    {
        _gateway = gateway;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<MuscleGetDTO>>> GetAllAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Invalidate();
        }
        else if (_cache.TryGetCompleted(out var cached))
        {
            return cached;
        }

        var result = await _cache.GetAllAsync(async () =>
        {
            var source = await _gateway.GetAllMusclesAsync();
            if (!source.Success)
            {
                return Result<IReadOnlyList<MuscleGetDTO>>.Failure(source.GetErrorMessage());
            }

            if (!source.HasValue || source.Value == null)
            {
                return Result<IReadOnlyList<MuscleGetDTO>>.SuccessResult(Array.Empty<MuscleGetDTO>());
            }
            
            return Result<IReadOnlyList<MuscleGetDTO>>.SuccessResult(source.Value);
        });

        if (result.Success && result.HasValue && result.Value != null)
        {
            Muscles = result.Value.ToDictionary(m => m.Id, m => m);
        }

        
        return result;
    }

    public void Invalidate() => _cache.Invalidate();

    public async Task<Result<MuscleGetDTO>> GetByIdAsync(Guid id, bool forceRefresh = false)
    {
        if (id == Guid.Empty)
        {
            return Result<MuscleGetDTO>.Failure("Invalid muscle id");
        }

        if (!forceRefresh && Muscles.TryGetValue(id, out var cached))
        {
            return Result<MuscleGetDTO>.SuccessResult(cached);
        }

        var listResult = await GetAllAsync(forceRefresh);
        if (!listResult.Success)
        {
            return Result<MuscleGetDTO>.Failure(listResult.GetErrorMessage());
        }

        var match = listResult.Value?.FirstOrDefault(m => m.Id == id);
        return match is null
            ? Result<MuscleGetDTO>.Failure($"Muscle {id} not found")
            : Result<MuscleGetDTO>.SuccessResult(match);
    }

    public async Task<MuscleGetDTO?> GetMuscleByIdAsync(Guid? id)
    {
        if (id is null || id == Guid.Empty)
        {
            return null;
        }

        if (Muscles.TryGetValue(id.Value, out var cached))
        {
            return cached;
        }

        var result = await GetAllAsync();
        return result.Success && Muscles.TryGetValue(id.Value, out var refreshed)
            ? refreshed
            : null;
    }
}
