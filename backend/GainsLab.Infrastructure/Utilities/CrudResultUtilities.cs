using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts;
using GainsLab.Domain;

namespace GainsLab.Infrastructure.Utilities;

public enum CrudOperation
{
    Create,
    Update,
    Delete,
    Get,
}

public static class CrudResultUtilities
{

    
    public static APIResult<TDto> DispatchResult<TDto, TRecord>(
        APIResult<TRecord> result,
        Func<TRecord, TDto> map,
        Guid? id = null,
        EntityType? type = null)
    {
        
        return result.Status switch
        {
            ApiResultStatus.Found => APIResult<TDto>.Found(map(result.Value!)),
            ApiResultStatus.Created => APIResult<TDto>.Created(map(result.Value!)),
            ApiResultStatus.Updated => APIResult<TDto>.Updated(map(result.Value!)),
            ApiResultStatus.NotFound => APIResult<TDto>.NotFound(result.GetErrorMessage()),
            ApiResultStatus.NotDeleted => APIResult<TDto>.NotDeleted(id ?? Guid.Empty, type ?? default, result.GetErrorMessage()),
            ApiResultStatus.BadRequest => APIResult<TDto>.BadRequest(result.GetErrorMessage()),
            ApiResultStatus.Unauthorized => APIResult<TDto>.Unauthorized(result.GetErrorMessage()),
            ApiResultStatus.Forbidden => APIResult<TDto>.Forbidden(result.GetErrorMessage()),
            ApiResultStatus.UnexpectedException => APIResult<TDto>.Exception(result.GetErrorMessage()),
            _ => throw new ArgumentOutOfRangeException(nameof(result.Status), result, null)
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