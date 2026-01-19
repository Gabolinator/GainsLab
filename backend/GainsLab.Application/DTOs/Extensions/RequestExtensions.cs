using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto.Request;

namespace GainsLab.Application.DTOs.Extensions;

public static class EquipmentCreateRequestExtensions
{

    public static Result IsCreateRequestValid(this EquipmentCreateRequest? request)
    {
        var messages = new MessagesContainer();
        
        if (request == null)
        {
            var message = "Request is null";
            messages.AddError(message);
            return Result.Failure(messages);
        }
        if (request.Equipment == null)
        {
            var message ="EquipmentPostDTO is null";
            messages.AddError(message);
            return Result.Failure(messages);
        }
        if (request.CreateRequest == CreateRequest.DontCreate)
        {
            var message ="Labeled as DontCreate";
            messages.AddError(message);
            return Result.Failure(messages);
        }

        return Result.SuccessResult();
    }
}

public static class MovementCategoryCreateRequestExtension
{

    public static  Result IsCreateRequestValid(this MovementCategoryCreateRequest? request)
    {
        var messages = new MessagesContainer();
        
        if (request == null)
        {
            var message = "Request is null";
            messages.AddError(message);
            return Result.Failure(messages);
        }
        if (request.MovementCategory== null)
        {
            var message ="MovementCategoryPostDTO is null";
            messages.AddError(message);
            return Result.Failure(messages);
        }
        if (request.CreateRequest == CreateRequest.DontCreate)
        {
            var message ="Labeled as DontCreate";
            messages.AddError(message);
            return Result.Failure(messages);
        }

        return Result.SuccessResult();
    }
}

public static class DescriptorCreateRequestExtension
{

    public static  Result IsCreateRequestValid(this DescriptorCreateRequest? request)
    {
        var messages = new MessagesContainer();
        
        if (request == null)
        {
            var message = "Request is null";
            messages.AddError(message);
            return Result.Failure(messages);
        }
        if (request.DescriptorPostDto == null)
        {
            var message ="DescriptorPostDTO is null";
            messages.AddError(message);
            return Result.Failure(messages);
        }
        if (request.CreateRequest == CreateRequest.DontCreate)
        {
            var message ="Labeled as DontCreate";
            messages.AddError(message);
            return Result.Failure(messages);
        }

        return Result.SuccessResult();
      }
}