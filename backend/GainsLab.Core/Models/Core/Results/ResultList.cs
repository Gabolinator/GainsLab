using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using GainsLab.Core.Models.Core.Utilities.Logging;

namespace GainsLab.Core.Models.Core.Results;


/// <summary>
/// Common surface for result collections.
/// </summary>
public interface IResultList
{
    public bool Success{ get; }

    public string? ErrorMessage { get; }

    public IReadOnlyList<string> Errors { get; }

    public bool HasError { get; }


    public int Count { get; }
    
    public bool AllSuccess { get; }
    public bool AllFailure { get; }
    public bool AnySuccess { get; }
    public bool AnyFailure { get; }
    
}

/// <summary>
/// Aggregates non-generic results while tracking errors.
/// </summary>
public class ResultList : IResultList,IResult, IEnumerable<Result>
{
    
    public ResultList()
    {
    }

    public ResultList(ILogger logger) => _logger = logger;

    public ResultList(IEnumerable<Result> results)
    {
        if (results is not null) _results.AddRange(results);
    }

    public ResultList(IEnumerable<Result> results, ILogger logger) : this(results)
        => _logger = logger;

    
    private readonly List<Result> _results = new();

    private ILogger? _logger;

    public virtual bool Success => !AllFailure;

    public string? ErrorMessage
        => Success
            ? null
            : string.Join(" | ",
                _results.Where(r => !r.Success)
                    .Select(r => r.GetErrorMessage())
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .DefaultIfEmpty("One or more items failed"));


    public IReadOnlyList<string> Errors
        => _results.Where(r => !r.Success)
            .Select(r => r.GetErrorMessage())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList()
            .AsReadOnly();


    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

 
    public int Count => _results.Count;


    public virtual void Add(Result result) => _results.Add(result);

    public virtual void AddRange(IEnumerable<Result> results) => _results.AddRange(results);

    public bool AllSuccess => _results.Count > 0 && _results.All(r => r.Success);
    public bool AllFailure => _results.Count > 0 && _results.All(r => !r.Success);
    public bool AnySuccess => _results.Any(r => r.Success);
    public bool AnyFailure => _results.Any(r => !r.Success);

    public IReadOnlyList<Result> Items => _results.AsReadOnly();

    public IEnumerator<Result> GetEnumerator() => _results.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();


    public static ResultList FailureResult(string message)
    {
        return new ResultList(new List<Result> { Result.Failure(message) });
        
    }

    public static ResultList SuccessResults(int count)
    {
        var list = new List<Result>();
        for (int i = 0; i < count; i++)
        {
            list.Add(Result.SuccessResult());
        }
        
        return new ResultList( list);
    }
}

/// <summary>
/// Aggregates typed results while tracking errors.
/// </summary>
public class ResultList<T> :  IResultList,IResult, IEnumerable<Result<T>> 
{
    
    public ResultList() { }

    public ResultList(ILogger logger) => _logger = logger;

    public ResultList(IEnumerable<Result<T>> results) 
    { 
        if (results is not null) _results.AddRange(results); 
    }

    public ResultList(IEnumerable<Result<T>> results, ILogger logger) : this(results)
        => _logger = logger;

    
    private readonly List<Result<T>> _results = new();

    private ILogger? _logger;

    public bool Success { get; }
   
    public string? ErrorMessage
        => Success
            ? null
            : string.Join(" | ",
                _results.Where(r => !r.Success)
                    .Select(r => r.GetErrorMessage())
                    .Where(m => !string.IsNullOrWhiteSpace(m))
                    .DefaultIfEmpty("One or more items failed"));

    
    public IReadOnlyList<string> Errors
        => _results.Where(r => !r.Success)
            .Select(r => r.GetErrorMessage())
            .Where(m => !string.IsNullOrWhiteSpace(m))
            .ToList()
            .AsReadOnly();


    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
    
  
    public int Count => _results.Count;
    
    
    public void Add(Result<T> result) => _results.Add(result);

    public void AddRange(IEnumerable<Result<T>> results) => _results.AddRange(results);

    public bool AllSuccess => _results.Count > 0 && _results.All(r => r.Success);
    public bool AllFailure => _results.Count > 0 && _results.All(r => !r.Success);
    public bool AnySuccess => _results.Any(r => r.Success);
    public bool AnyFailure => _results.Any(r => !r.Success);

    public IReadOnlyList<Result<T>> Items => _results.AsReadOnly();

    public IEnumerator<Result<T>> GetEnumerator() => _results.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

    public bool TryGetSuccessValues(
        [NotNullWhen(true)] out IReadOnlyList<T>? values)
        => TryGetSuccessValuesCore(_logger, out values);
    
    public bool TryGetSuccessValues(
        ILogger? logger,
        [NotNullWhen(true)] out IReadOnlyList<T>? values)
        => TryGetSuccessValuesCore(logger, out values);
    
    
    private bool TryGetSuccessValuesCore(
        ILogger? logger,
        [NotNullWhen(true)] out IReadOnlyList<T>? values)
    {
        if (_results.Count == 0 ||  AllFailure)
        {
            if(_logger != null) _logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: No input data.");
            values = null;
            return false;
        }

        var list = new List<T>(_results.Count);

        foreach (var r in _results)
        {
            if (!r.Success)
            {
                if(_logger != null) _logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: the result failed. Message: { r.GetErrorMessage()}");
                continue;
            }

            if (!r.TryGetValue(out var value))
            {
                if(_logger != null) _logger.LogWarning(typeof(T).Name,$"Could not map Result to {typeof(T).Name}: success but no value.");
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


    public static ResultList<T> FailureResult<T>(string message)
    {
        var result = new ResultList<T>(new List<Result<T>> { Result<T>.Failure(message) });
        return result;
    }

    public static ResultList<T> SuccessResults<T>(IEnumerable<T> values)
    {
       
        var list = new List<Result<T>>();
        for (int i = 0; i < values.Count(); i++)
        {
            var obj = values.ElementAtOrDefault(i);
            if (obj == null) continue;
            
            list.Add(Result<T>.SuccessResult(obj));
        }
        
        return new ResultList<T>(list);
    }
    

    
}
