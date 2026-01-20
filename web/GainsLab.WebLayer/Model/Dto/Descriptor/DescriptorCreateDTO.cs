using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Infrastructure.Utilities;
using ILogger = GainsLab.Domain.Interfaces.ILogger;

namespace GainsLab.WebLayer.Model.Dto.Descriptor;

public class DescriptorCreateDTO
{
    public Guid  Id { get; init; } = Guid.NewGuid();
    
    [StringLength(1024, MinimumLength = 3)]
    public string? DescriptionContent { get; set; } = "new description";

    [StringLength(2048)] public string? Notes { get; set; } = null;

    [MaxLength(20)] public ICollection<string>? Tags { get; set; } = null;
    
    public string? CreatedBy { get; set; }
    
    public CreateRequest CreateRequest { get; set; } = CreateRequest.Create;
    
}


public static class DescriptorCreateDTOExtensions
{
    public static Result IsValid(this DescriptorCreateDTO dto ,  ILogger? logger = null)
    {
        if(dto.Id ==  Guid.Empty) return Result.Failure("Invalid Id");
        var content = StringFormater.RemoveUnwantedChar(dto.DescriptionContent);
        var comparison = "new description";
        var equals = string.Equals(
            content,
            StringFormater.RemoveUnwantedChar(comparison),
            StringComparison.InvariantCultureIgnoreCase);

        if(logger != null)logger.Log("DescriptorCreateDTO.IsValid()", $"content {content} - comparison :{comparison} - equals: {equals}");
        
        if(string.IsNullOrEmpty(content)) return Result.Failure("Description cannot be empty");

        if (equals) return Result.Failure("Enter a description");

        return Result.SuccessResult();
        
    }
    
    public static DescriptorCreateRequest? ToCreateRequest(this DescriptorCreateDTO? dto)
    {
       
        if(dto == null || dto.ToPostDTO() == null) return null;
        
        return  new DescriptorCreateRequest(dto.ToPostDTO()!, dto.CreateRequest,dto.CreatedBy?? "unknown" );
    }

    
    
    public static DescriptorPostDTO? ToPostDTO(this DescriptorCreateDTO? dto)
    {
        if(dto == null) return null;
        
        return new DescriptorPostDTO
        {
            Id = dto.Id,
            DescriptionContent =  dto.DescriptionContent ?? string.Empty,
            Notes = dto.Notes,
            Tags = dto.Tags?.ToList(),
            CreatedBy = dto.CreatedBy
        };
    }
}
