using GainsLab.Models.Core.Results;

namespace GainsLab.Models.Factory;

public static class Results
{
    
    public static Result<bool> SuccessResult() =>
        new Result<bool>(true);
    public static Result<T> SuccessResult<T>() =>
        new Result<T>(true);
    
    public static Result<T> SuccessResult<T>(T value) =>
        new Result<T>(true, value);

    
    public static Result<T> NotImplementedResult<T>(string errorMessage ="") =>
         FailureResult<T>($"Not implemented : {errorMessage}");
    
    public static Result<bool> FailureResult(string errorMessage) =>
        new Result<bool>(false, default, errorMessage);
    
    public static Result<T> FailureResult<T>(string errorMessage) =>
        new Result<T>(false, default, errorMessage);

    public static Result<T> ComponentNotFoundResult<T>(string? message = null) =>
        new Result<T>(false, default, message ?? "Component not found");
    
  
}