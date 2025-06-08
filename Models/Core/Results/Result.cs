namespace GainsLab.Models.Core.Results;

public readonly struct Result<T>
{
    public bool Success { get; }
    public T? Value { get; }
    public string? ErrorMessage { get; }

    public bool HasError => !string.IsNullOrWhiteSpace(ErrorMessage);

    public Result(bool success, T? value = default, string? errorMessage = null)
    {
        Success = success;
        Value = value;
        ErrorMessage = errorMessage;
    }

    public void Deconstruct(out bool success, out T? value, out string? error)
    {
        success = Success;
        value = Value;
        error = ErrorMessage;
    }

    public override string ToString()
    {
        return Success
            ? $"Success: {Value}"
            : $"Failure: {ErrorMessage ?? "Unknown Error"}";
    }


    
}