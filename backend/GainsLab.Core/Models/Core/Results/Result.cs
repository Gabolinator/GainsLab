using System.Diagnostics.CodeAnalysis;

namespace GainsLab.Core.Models.Core.Results;


/// <summary>
/// Represents the outcome of an operation without exposing typed values.
/// </summary>
public interface IResult
{
    bool Success { get; }
    string? ErrorMessage { get; }
}

/// <summary>
/// Basic success/failure result with optional error messaging.
/// </summary>
public class Result : IResult
{
    public bool Success { get;}
    public string? ErrorMessage { get; }
    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    protected Result(bool success, string? errorMessage = null)
    {
        Success = success;
        ErrorMessage = errorMessage;
    }

    public static Result SuccessResult() => new(true);
    public static Result Failure(string errorMessage)
        => new(false, string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage);

    public override string ToString() =>
        Success ? "Success" : $"Failure: {ErrorMessage ?? "Unknown Error"}";
    
    
    public virtual string GetErrorMessage() => HasError ? ErrorMessage! : string.Empty;
}

/// <summary>
/// Success/failure result that can optionally carry a value of type <typeparamref name="T"/>.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    public override string GetErrorMessage() => HasError ? ErrorMessage! : string.Empty;

    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    public Result(bool success, T? value, string? errorMessage)
        : base(success, errorMessage)
    {
        Value = value;
    }

    public static Result<T> SuccessResult(T value)
        => new(true, value, null);

    public static Result<T> Failure(string errorMessage)
        => new(false, default, string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage);

    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = Success ? Value : default;
        return Success && Value is not null;
    }

    public void Deconstruct(out bool success, out T? value, out string? error)
    {
        success = Success;
        value = Value;
        error = ErrorMessage;
    }

    public override string ToString()
        => Success ? $"Success: {Value}" : $"Failure: {ErrorMessage ?? "Unknown Error"}";
}

// public class Result : IResult
// {
//     public bool Success { get; }
//     public string? ErrorMessage { get; }
//     public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
//
//     protected Result(bool success, string? errorMessage = null)
//     {
//         Success = success;
//         ErrorMessage = errorMessage;
//     }
//
//     public static Result SuccessResult() => new(true);
//     public static Result Failure(string errorMessage) => new(false, errorMessage);
//
//     public override string ToString() =>
//         Success ? "Success" : $"Failure: {ErrorMessage ?? "Unknown Error"}";
// }
//
//
// public class Result<T> : IResult
// {
//     public T? Value { get; }
//     
//     public bool Success { get; }
//    
//     public string? ErrorMessage { get; }
//
//     public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);
//
//     public string GetMessage() => HasError ? ErrorMessage! : string.Empty;
//     // If you want: success implies non-null Value for ref types
//     [MemberNotNullWhen(true, nameof(Value))]
//     public bool HasValue => Value is not null;
//
//
//
//     private Result(bool success, T? value, string? errorMessage)
//     {
//         Success = success;
//         Value = value;
//         ErrorMessage = errorMessage;
//     }
//
//     public static Result<T> SuccessResult(T value)
//         => new(true, value, null);
//
//     public static Result<T> Failure(string errorMessage)
//         => new(false, default, string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage);
//
//     public bool TryGetValue([NotNullWhen(true)] out T? value)
//     {
//         value = Success ? Value : default;
//         return Success && Value is not null;
//     }
//
//     public void Deconstruct(out bool success, out T? value, out string? error)
//     {
//         success = Success;
//         value = Value;
//         error = ErrorMessage;
//     }
//
//     public override string ToString()
//         => Success ? $"Success: {Value}" : $"Failure: {ErrorMessage ?? "Unknown Error"}";
// }


