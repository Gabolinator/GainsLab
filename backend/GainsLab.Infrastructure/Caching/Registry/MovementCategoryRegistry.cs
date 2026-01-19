using System.Collections.Generic;
using System.Linq;
using GainsLab.Application.Interfaces.DataManagement.Gateway;
﻿using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Infrastructure.Caching.QueryCache;

namespace GainsLab.Infrastructure.Caching.Registry;

public sealed class MovementCategoryRegistry
{
    private readonly IMovementCategoryGateway _gateway;
    private readonly MovementCategoryQueryCache _cache;

    public Dictionary<Guid, MovementCategoryGetDTO> MovementCategories { get; private set; } = new();

    public MovementCategoryRegistry(IMovementCategoryGateway gateway, MovementCategoryQueryCache cache)
    {
        _gateway = gateway;
        _cache = cache;
    }

    public async Task<Result<IReadOnlyList<MovementCategoryGetDTO>>> GetAllAsync(bool forceRefresh = false)
    {
        if (forceRefresh)
        {
            _cache.Invalidate();
        }
        else if (_cache.TryGetCompleted(out var cached))
        {
            return cached;
        }

        var result = await _cache.GetAllAsync(() => _gateway.GetAllCategoryAsync());
        
        if (result.Success && result.HasValue && result.Value is not null)
        {
            MovementCategories = result.Value.ToDictionary(category => category.Id, category => category);
        }

        return result;
    }

    public void Invalidate()
    {
        _cache.Invalidate();
        MovementCategories.Clear();
    }
    

    public async Task<Result<MovementCategoryGetDTO>> GetByIdAsync(Guid id, bool forceRefresh = false)
    {
        if (id == Guid.Empty)
        {
            return Result<MovementCategoryGetDTO>.Failure("Invalid movement category id");
        }

        if (!forceRefresh && MovementCategories.TryGetValue(id, out var cached))
        {
            return Result<MovementCategoryGetDTO>.SuccessResult(cached);
        }

        var listResult = await GetAllAsync(forceRefresh);
        if (!listResult.Success)
        {
            return Result<MovementCategoryGetDTO>.Failure(listResult.GetErrorMessage());
        }

        var match = listResult.Value?.FirstOrDefault(category => category.Id == id);
        return match is null
            ? Result<MovementCategoryGetDTO>.Failure($"Movement category {id} not found")
            : Result<MovementCategoryGetDTO>.SuccessResult(match);
    }

    public async Task<MovementCategoryGetDTO?> GetMovementCategoryByIdAsync(Guid? id)
    {
        if (id is null || id == Guid.Empty)
        {
            return null;
        }

        if (MovementCategories.TryGetValue(id.Value, out var cached))
        {
            return cached;
        }

        var result = await GetAllAsync();
        return result.Success && MovementCategories.TryGetValue(id.Value, out var refreshed)
            ? refreshed
            : null;
    }

    public async Task<Result<MovementCategoryDeleteOutcome>> DeleteCategoryAsync(
        MovementCategoryEntityId movementCategoryEntityId)
    {
        var result = await _gateway.DeleteMovementCategoryAsync(movementCategoryEntityId, _cache);
        if (result.Success)
        {
            Invalidate();
            if (movementCategoryEntityId.Id.HasValue)
            {
                MovementCategories.Remove(movementCategoryEntityId.Id.Value);
            }
        }

        return result;
    }

    public async Task<Result<MovementCategoryUpdateCombinedOutcome>> UpdateMovementCategoryAsync(
        MovementCategoryUpdateRequest request,
        DescriptorUpdateRequest? descriptorRequest)
    {
        var result = await _gateway.UpdateMovementCategoryAsync(request, descriptorRequest, _cache);
        if (result.Success)
        {
            Invalidate();
        }

        return result;
    }

    public async Task<Result<MovementCategoryCreateCombineOutcome>> CreateMovementCategoryAsync(
        MovementCategoryCombineCreateRequest request)
    {
        var result = await _gateway.CreateMovementCategoryAsync(request, _cache);
        if (result.Success)
        {
            Invalidate();
        }

        return result;
    }

  
}
