namespace GainsLab.Contracts.Dtos.PostDto.Request;

public sealed record DescriptorCreateRequest(
    DescriptorPostDTO? DescriptorPostDto,
    CreateRequest CreateRequest,
    string RequestedBy);


public static class DescriptorCreateRequestExtensions
{
    public static string Print(this DescriptorCreateRequest request)
    {
        if (request == null)
        {
            return "DescriptorCreateRequest <null>";
        }

        var descriptor = request.DescriptorPostDto is null
            ? "null"
            : $"{{ Id: {request.DescriptorPostDto.Id}, DescriptionContent: \"{request.DescriptorPostDto.DescriptionContent}\", Notes: \"{request.DescriptorPostDto.Notes ?? "null"}\", Tags: [{(request.DescriptorPostDto.Tags is { Count: > 0 } tags ? string.Join(", ", tags) : "none")}], Authority: {request.DescriptorPostDto.Authority}, CreatedBy: {request.DescriptorPostDto.CreatedBy ?? "null"} }}";

        return
            $"DescriptorCreateRequest {{ CreateRequest: {request.CreateRequest}, RequestedBy: \"{request.RequestedBy}\", Descriptor: {descriptor} }}";
    }
    
}
