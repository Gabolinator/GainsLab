namespace GainsLab.Application.Results.APIResults;


public enum ApiResultStatus
{
    Found,
    Updated,
    Created,
    Deleted,
    NotFound,
    BadRequest,
    Unauthorized,
    Forbidden,
    UnexpectedException
}




public class APIResult<T> : Result<T>
{
    
    public APIResult(bool success, T? value, ApiResultStatus actionResult, string? errorMessage) : base(success, value, errorMessage)
    {
        Status = actionResult;
    }

  

    public ApiResultStatus Status { get; set; }
    
    
    public static APIResult<T> Found(T value)
        => SuccessResult(ApiResultStatus.Found, value);
    
    public static APIResult<T> Created(T value)
        => SuccessResult(ApiResultStatus.Created, value);
    
    
    public static APIResult<T> Updated(T value)
        => SuccessResult(ApiResultStatus.Updated, value);
    
    public static APIResult<T> Deleted(T value)
        => SuccessResult(ApiResultStatus.Deleted, value);
    
    public static APIResult<T> NotFound(string notFoundMessage) 
        => Failure(ApiResultStatus.NotFound, $"Not Found: {notFoundMessage}" );
    
    public static APIResult<T> Unauthorized(string unauthorizedMessage = "Access Denied") =>
        Failure(ApiResultStatus.Unauthorized, unauthorizedMessage); 
    
    public static APIResult<T> Forbidden(string unauthorizedMessage = "Access Denied") =>
        Failure(ApiResultStatus.Forbidden, unauthorizedMessage); 
    
    public static APIResult<T> BadRequest(string badRequestMessage = "Bad Request") => 
        Failure(ApiResultStatus.BadRequest, badRequestMessage);
    
    public static APIResult<T> NotCreated(string exception) => 
        Problem($"Not Created : {exception} ");
    
    public static APIResult<T> NothingChanged(string exception) => 
        BadRequest($"Nothing changed {exception}");
    
    public static APIResult<T> NotUpdated(string exception) => 
        NotFound($"Not Updated : {exception} ");
    
    public static APIResult<T> Exception(string exception) => 
        Problem(exception);
    
    public static APIResult<T> Problem(string exception) => 
        Failure(ApiResultStatus.UnexpectedException, $"Unexpected Exception: {exception}");
    
    
    
    public new static APIResult<T> SuccessResult(ApiResultStatus actionResult,T value)
        => new(true, value, actionResult , null);
    
    
    public  new static APIResult<T> Failure(ApiResultStatus actionResult ,string errorMessage)
        => new(false, default, actionResult, errorMessage);

   

   
    
    
}

