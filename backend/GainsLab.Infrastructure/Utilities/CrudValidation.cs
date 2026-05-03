using GainsLab.Application.Results.APIResults;
using GainsLab.Domain;

namespace GainsLab.Infrastructure.Utilities;

public static class CrudValidation
{
    
    public static async Task<UniqueValidationResult<TDto>> TryValidateUniqueAsync<TEntity, TDto>(
        TEntity entity,
        EntityType entityType,
        Func<TEntity, Guid?> getId,
        Func<TEntity, string?> getName,
        Func<TEntity, string?> getContent,
        Func<TEntity, string?> getOther,
        Func<TEntity, CancellationToken, Task<MatchingResult<TEntity>>> getExistingRecordAsync,
        CancellationToken ct)
    {
        var existing = await getExistingRecordAsync(entity, ct);

        if (!existing.MatchFound)
        {
            return UniqueValidationResult<TDto>.Success();
        }

        var message = GetConflictMessage(
            existing.MatchCause,
            entityType,
            getId(entity),
            getName(entity),
            getContent(entity),
            getOther(entity));

        var result = APIResult<TDto>.NotCreated(
            message,
            NotCreatedReason.Conflict);

        return UniqueValidationResult<TDto>.Conflict(result);
    }
    
    public static string GetConflictMessage(
        MatchCause cause,
        EntityType entityType,
        Guid? guid,
        string? name,
        string? content,
        string? other)
    {
        var message = $"Found conflict {entityType} entity";
        
        return cause switch
        {
            MatchCause.SameId => $"{message} with same id '{guid}'",
            MatchCause.SameName =>  $"{message} with same name '{name}'",
            MatchCause.SameContent =>  $"{message} with same content '{content}'",
            MatchCause.Other => $"{message} with '{other}'",
            MatchCause.None => string.Empty,
            _ => throw new ArgumentOutOfRangeException(nameof(cause), cause, null)
        };
    }
    
    public static APIResult<T>? ValidateId<TPayload, T>(
        TPayload entity,
        EntityType type,
        Func<TPayload, Guid?> getId)
    {
        var id = getId(entity);
        if (id == null || id == Guid.Empty)
            return APIResult<T>.BadRequest($"{type} ID cannot be null or empty");

        return null;
    }
    
    public static APIResult<T>? ValidatePayload<TPayload, T>(
        TPayload? payload,
        EntityType type,
        Func<TPayload, Guid?> getId)
    {
        if (payload is null)
            return APIResult<T>.BadRequest($"Payload for {type} cannot be null");

        var idValidation = ValidateId<TPayload, T>(payload, type, getId);
        
        if (idValidation != null)
            return idValidation;
        

        return null;
    }

    public static APIResult<T>? ValidateRequiredText<T>(
        string? value,
        string fieldName,
        Func<string, string>? getFormatedText)
    {
        var text = getFormatedText?.Invoke(value ?? string.Empty) ?? value;
        
        if (string.IsNullOrWhiteSpace(text))
            return APIResult<T>.BadRequest($"{fieldName} cannot be empty");

        return null;
    }

    public static EntityBuildResult<TEntity, TReturn>
        ValidatePayloadAndBuildEntity<TPayload, TEntity, TReturn>(
            TPayload? payload,
            EntityType type,
            Func<TPayload, Guid?>? getId,
            Func<TPayload, TEntity?> buildEntity)
    {
        if (payload is null)
        {
            return EntityBuildResult<TEntity, TReturn>.Fail(
                APIResult<TReturn>.BadRequest($"Payload for {type} cannot be null"));
        }

        var id = getId?.Invoke(payload);

        if (getId is not null && id is null || id == Guid.Empty)
        {
            return EntityBuildResult<TEntity, TReturn>.Fail(
                APIResult<TReturn>.BadRequest($"{type} ID cannot be null or empty"));
        }

        var entity = buildEntity(payload);

        if (entity is null)
        {
            return EntityBuildResult<TEntity, TReturn>.Fail(
                APIResult<TReturn>.BadRequest($"Could not construct {type} entity from payload"));
        }

        return EntityBuildResult<TEntity, TReturn>.Ok(entity);
    }
}

public sealed record EntityBuildResult<TEntity, TReturn>
{
    public bool Success { get; init; }
    public TEntity? Entity { get; init; }
    public APIResult<TReturn>? Error { get; init; }

    public static EntityBuildResult<TEntity, TReturn> Ok(TEntity entity) => new()
    {
        Success = true,
        Entity = entity
    };

    public static EntityBuildResult<TEntity, TReturn> Fail(APIResult<TReturn> error) => new()
    {
        Success = false,
        Error = error
    };
}