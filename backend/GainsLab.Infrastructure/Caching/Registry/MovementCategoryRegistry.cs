using GainsLab.Application.Interfaces.DataManagement.Gateway;
using GainsLab.Application.Interfaces.DataManagement.Provider;
﻿using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.Delete.Outcome;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.ID;
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


    public async Task<Result<MovementCategoryDeleteOutcome>> DeleteCategoryAsync(MovementCategoryEntityId movementCategoryEntityId)
    {
       
    }

    public async Task<Result<IReadOnlyList<MovementCategoryGetDTO>>> GetAllAsync()
    {
      
    }
}
