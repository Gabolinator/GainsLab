using Microsoft.AspNetCore.Mvc;

namespace GainsLab.Application.Results.APIResults;

public record ActionResultInfo(string nameOfAction, Guid id)
{
       
}

public static class APIResultValidation
{
    
    public static IActionResult ValidateResult<T>(ControllerBase controller, APIResult<T> result, ActionResultInfo? actionResultInfo = null)
    {
        return (result.Status) switch
        {
            ApiResultStatus.Found or ApiResultStatus.Updated => result.Value != null
                ? controller.Ok(result.Value!)
                : controller.NotFound(),
            ApiResultStatus.Created => result.Value != null && actionResultInfo != null
                ? controller.CreatedAtAction(
                    actionResultInfo.nameOfAction, 
                    new { id = actionResultInfo.id}, 
                    result.Value)
                : controller.Problem(),
            ApiResultStatus.Deleted => controller.NoContent(),
            ApiResultStatus.NotFound => controller.NotFound(result.GetErrorMessage()),
            ApiResultStatus.BadRequest => controller.BadRequest(result.GetErrorMessage()),
            ApiResultStatus.Unauthorized => controller.Unauthorized(result.GetErrorMessage()),
            ApiResultStatus.Forbidden => controller.Forbid(),
            _ => controller.Problem(result.GetErrorMessage())
        };
    }


}

