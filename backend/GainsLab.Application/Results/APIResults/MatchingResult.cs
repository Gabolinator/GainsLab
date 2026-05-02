namespace GainsLab.Application.Results.APIResults;

public record MatchingResult<T>
{
    public APIResult<T>? Result { get; init; }
    public MatchCause MatchCause { get; init; } =  MatchCause.None;
    public bool MatchFound => MatchCause !=  MatchCause.None && Result != null && Result.Value != null;

    public static MatchingResult<T> FoundById(T value) => new()
    {
        Result = APIResult<T>.Found(value),
        MatchCause = MatchCause.SameId,
    };
   
    public static MatchingResult<T> FoundByName(T value) => new()
    {
        Result = APIResult<T>.Found(value),
        MatchCause = MatchCause.SameName,
    };
   
    public static MatchingResult<T> FoundByContent(T value) => new()
    {
        Result = APIResult<T>.Found(value),
        MatchCause = MatchCause.SameContent,
    };
   
    public static MatchingResult<T> FoundByOther(T value) => new()
    {
        Result = APIResult<T>.Found(value),
        MatchCause = MatchCause.Other,
    };
   
    public static MatchingResult<T> NoMatchFound(
        Guid? id,
        string? name,
        string? content,
        string? other)
    {
        var parts = new List<string>();

        if (id.HasValue)
            parts.Add($"id '{id}'");

        if (!string.IsNullOrWhiteSpace(name))
            parts.Add($"name '{name}'");

        if (!string.IsNullOrWhiteSpace(content))
            parts.Add($"content '{content}'");

        if (!string.IsNullOrWhiteSpace(other))
            parts.Add($"other '{other}'");

        var criteria = parts.Count switch
        {
            0 => "with no identifying information",
            1 => $"with {parts[0]}",
            _ => $"with {string.Join(" or ", parts)}"
        };

        return new MatchingResult<T>
        {
            Result = APIResult<T>.NotFound(
                $"Did not find {typeof(T).Name} entity {criteria}"
            ),
            MatchCause = MatchCause.None,
        };
    }
}