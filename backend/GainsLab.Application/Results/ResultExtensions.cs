using System.Diagnostics.CodeAnalysis;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.Results;

/// <summary>
/// Helper methods for extracting data from result collections.
/// </summary>
public static class ResultExtensions
{


    public static ResultList ToBoolResultList<T>(this ResultList<T> results)
    {
        if (results == null || results.Count == 0) return ResultList.FailureResult("No data");

        var resultList = new ResultList();

        resultList.AddRange(results);

        return resultList;

    }

    public static bool TryGetSuccessValues<T>(this IEnumerable<Result<T>> results,
        ILogger? logger,
        [NotNullWhen(true)] out IReadOnlyList<T>? values)
    {
        if (!results.Any())
        {
            if(logger != null) logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: No input data.");
            values = null;
            return false;
        }

        var list = new List<T>(results.Count());

        foreach (var r in results)
        {
            if (!r.Success)
            {
                if(logger != null) logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: the result failed. Message: { r.GetErrorMessage()}");
                continue;
            }

            if (!r.TryGetValue(out var value))
            {
                if(logger != null) logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: success but no value.");
                continue;
            }

            list.Add(value);
        }

        if (list.Count == 0)
        {
            values = null;
            return false;
        }

        values = list;
        return true;
    }
    
    public static List<T> ToValues<T>(this IEnumerable<Result<T>> results, ILogger logger)
    {
        if (results is null) throw new ArgumentNullException(nameof(results));

        var any = false;
        var list = new List<T>();

        foreach (var r in results)
        {
            any = true;

            if (!r.Success)
            {
               if(logger != null) logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: the result failed. Message: { r.GetErrorMessage()}");
                continue;
            }

            if (!r.TryGetValue(out T? value))
            {
                if(logger != null) logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: success but no value.");
                continue;
            }
            
            list.Add(value!);
        }

        if (!any)
        {
            if(logger != null) logger.LogWarning("Could not map Results to {Type}: input sequence is empty.", typeof(T).Name);
        }

        return list;
    }
}
