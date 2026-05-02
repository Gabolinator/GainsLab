using GainsLab.Application.Results.APIResults;
using GainsLab.Domain;

namespace GainsLab.Infrastructure.Utilities;

public static class CrudResultUtilities
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
    
    public static APIResult<TDto> DispatchResult<TDto, TRecord>(
        APIResult<TRecord> result,
        Func<TRecord, TDto> map)
    {
        return result.Status switch
        {
            ApiResultStatus.Found => APIResult<TDto>.Found(map(result.Value!)),
            ApiResultStatus.Created => APIResult<TDto>.Created(map(result.Value!)),
            ApiResultStatus.Updated => APIResult<TDto>.Updated(map(result.Value!)),
            ApiResultStatus.NotFound => APIResult<TDto>.NotFound(result.GetErrorMessage()),
            ApiResultStatus.BadRequest => APIResult<TDto>.BadRequest(result.GetErrorMessage()),
            ApiResultStatus.Unauthorized => APIResult<TDto>.Unauthorized(result.GetErrorMessage()),
            ApiResultStatus.Forbidden => APIResult<TDto>.Forbidden(result.GetErrorMessage()),
            ApiResultStatus.UnexpectedException => APIResult<TDto>.Exception(result.GetErrorMessage()),
            _ => throw new ArgumentOutOfRangeException(nameof(result.Status), result, null)
        };
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
    
    public static async Task<MatchingResult<T>> GetExistingRecordAsync<T>(
        Guid? id,
        string? name,
        string? content,
        string? other,
        Func<Guid?, CancellationToken, Task<APIResult<T>>>? getById,
        Func<string, CancellationToken, Task<APIResult<T>>>? getByName,
        Func<string, CancellationToken, Task<APIResult<T>>>? getByContent,
        Func<string, CancellationToken, Task<APIResult<T>>>? getByOther,
        CancellationToken ct)
    {
        if (id.HasValue && getById != null)
        {
            var result = await getById(id, ct);
            if (result is { Success: true, Value: not null })
                return MatchingResult<T>.FoundById(result.Value);
        }

        if (!string.IsNullOrWhiteSpace(name) && getByName != null)
        {
            var result = await getByName(name, ct);
            if (result is { Success: true, Value: not null })
                return MatchingResult<T>.FoundByName(result.Value);
        }

        if (!string.IsNullOrWhiteSpace(content) && getByContent != null)
        {
            var result = await getByContent(content, ct);
            if (result is { Success: true, Value: not null })
                return MatchingResult<T>.FoundByContent(result.Value);
        }

        if (!string.IsNullOrWhiteSpace(other) && getByOther != null)
        {
            var result = await getByOther(other, ct);
            if (result is { Success: true, Value: not null })
                return MatchingResult<T>.FoundByOther(result.Value);
        }

        return MatchingResult<T>.NoMatchFound(id, name, content, other);
    }
}