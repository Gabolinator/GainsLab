namespace GainsLab.Application.Results.APIResults;

public record UniqueValidationResult<T>
{
    public bool AlreadyExists { get; init; }
    public APIResult<T>? ExistingResult { get; init; }

    public static UniqueValidationResult<T> Success() => new()
    {
        AlreadyExists = false,
        ExistingResult = null
    };

    public static UniqueValidationResult<T> Conflict(APIResult<T> result) => new()
    {
        AlreadyExists = true,
        ExistingResult = result
    };
}