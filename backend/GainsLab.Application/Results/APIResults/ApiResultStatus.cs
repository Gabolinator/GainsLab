namespace GainsLab.Application.Results.APIResults;


    public enum ApiResultStatus
    {
        Found,
        Updated,
        Created,
        Conflict,
        Deleted,
        NotFound,
        BadRequest,
        Unauthorized,
        Forbidden,
        UnexpectedException,
        Ok, 
    }
