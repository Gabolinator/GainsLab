using System.Diagnostics.CodeAnalysis;
using GainsLab.Application.DTOs.Muscle;

namespace GainsLab.Application.Results;

/// <summary>
/// Represents the outcome of an operation without exposing typed values.
/// </summary>
public interface IResult
{
    bool Success { get; }
    MessagesContainer Messages { get; }
}



/// <summary>
/// Basic success/failure result with optional error messaging.
/// </summary>
public class Result : IResult
{
    public bool Success { get;}
    public MessagesContainer Messages { get; } = new MessagesContainer();

    public bool HasError => Messages.HasErrors;

    public void AddError(string? errorMessage)
    {
        if(string.IsNullOrWhiteSpace(errorMessage)) return;
        Messages.AddError(errorMessage);
    }
    public void AddInfo(string? message)
    {
        if(string.IsNullOrWhiteSpace(message)) return;
        Messages.AddInfo(message);
    }

    protected Result(bool success, string? errorMessage = null)
    {
        Success = success;
        AddError(errorMessage);
    }
   
    
    protected Result(bool success, MessagesContainer? message = null)
    {
        Success = success;
        Messages = message ?? new MessagesContainer();
    }

    public static Result SuccessResult() => new(true, new MessagesContainer());
    public static Result Failure(string errorMessage)
        => new(false, string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage);

    public override string ToString() =>
        Success ? "Success" : $"Failure: {GetErrorMessage() ?? "Unknown Error"}";
    
    
    public virtual string GetErrorMessage() => HasError ? Messages.ToString(MessageType.Error)! : string.Empty;
}

/// <summary>
/// Success/failure result that can optionally carry a value of type <typeparamref name="T"/>.
/// </summary>
public class Result<T> : Result
{
    public T? Value { get; }

    
    [MemberNotNullWhen(true, nameof(Value))]
    public bool HasValue => Value is not null;

    public Result(bool success, T? value, string? errorMessage)
        : base(success, errorMessage)
    {
        Value = value;
    }
    
    public Result(bool success, T? value, MessagesContainer? message)
        : base(success, message)
    {
        Value = value;
    }

    public static Result<T> NotImplemented(string?  context =null)
        => Result<T>.Failure($"{(string.IsNullOrWhiteSpace(context)? "" : context + "- ")}Not Implemented");
    
    public static Result<T> SuccessResult(T value)
        => new(true, value, new MessagesContainer());

    public static Result<T> Failure(string errorMessage)
        => new(false, default, string.IsNullOrWhiteSpace(errorMessage) ? "Unknown error" : errorMessage);

    public static Result<T> Failure(MessagesContainer message)
        => new(false, default, message);
    
    public bool TryGetValue([NotNullWhen(true)] out T? value)
    {
        value = Success ? Value : default;
        return Success && Value is not null;
    }

   

    public override string ToString()
        => Success ? $"Success: {Value}" : $"Failure: {Messages.ToString() ?? "Unknown Error"}";
    
    public MessagesContainer GetMessages()
    {
        return Messages;
    }
}



