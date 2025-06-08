using GainsLab.Models.Core.Results;

namespace GainsLab.Models.Factory;

public static class ResultsFactory
{
    public static Result<T> Success<T>(T value) =>
        new Result<T>(true, value);

    public static Result<T> Failure<T>(string errorMessage) =>
        new Result<T>(false, default, errorMessage);

    public static Result<T> NotFound<T>(string? message = null) =>
        new Result<T>(false, default, message ?? "Component not found");
}