namespace GainsLab.Application.Results.APIResults;


    public enum ApiResultStatus
    {
        Found,
        Updated,
        Created,
        Conflict,
        Deleted,
        NotDeleted,
        NotFound,
        BadRequest,
        Unauthorized,
        Forbidden,
        UnexpectedException,
        Ok, 
    }
