using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Domain;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Muscle;

public class MuscleCreateDTO : MuscleFormDTO
{
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorCreateDTO();
    public override FormType FormType => FormType.Edit;
    
    public string? CreateBy => FilledBy;
    
    public CreateRequest CreateRequest =>  
        ApplyRequest == Request.ApplyRequest ? CreateRequest.Create :CreateRequest.DontCreate;

    
    
    public override string ToString()
    {
        return $"{Id.ToString()} -  {Name} - Antagonists : {string.Join(',', Antagonists)}";
    }
}


public static class MuscleCreateDTOExtension
{
    public static Result IsValid(this MuscleCreateDTO muscle, Domain.Interfaces.ILogger? logger = null)
    {
        if (muscle.Id == Guid.Empty) return Result.Failure("Invalid Id");
        var name = StringFormater.RemoveUnwantedChar(muscle.Name);
        var comparison = "new name";
        var equals = string.Equals(
            StringFormater.RemoveUnwantedChar(muscle.Name),
            StringFormater.RemoveUnwantedChar(comparison),
            StringComparison.InvariantCultureIgnoreCase);

        if (logger != null)
            logger.Log("MovementCategoryCreateDTO.IsValid()",
                $"name {name} - comparison :{comparison} - equals: {equals}");

        if (string.IsNullOrEmpty(muscle.Name) || equals)
            return Result.Failure("Invalid Name");

        return Result.SuccessResult();

    }

    

    public static MuscleCombineCreateRequest ToCombineCreateRequest(this MuscleCreateDTO muscle,
        Domain.Interfaces.ILogger? logger = null)
    {
        //
        var descriptor =  muscle.Descriptor is DescriptorCreateDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorCreateDTO();
        return new MuscleCombineCreateRequest( muscle.ToCreateRequest(), descriptor.ToCreateRequest());
    }

    public static string Print(this MuscleCombineCreateRequest request)
    {
        if (request == null)
        {
            return "MuscleCombineCreateRequest <null>";
        }

        return
            $"MuscleCombineCreateRequest {{ Muscle: {request.Muscle.Print()}, Descriptor: {request.Descriptor.Print()} }}";
    }
    
    public static string Print(this MuscleCreateRequest request)
    {
        if (request == null)
        {
            return "MuscleCreateRequest <null>";
        }

        return
            $"MuscleCreateRequest {{ Request: {request.CreateRequest}, FilledBy: \"{request.RequestedBy ?? "unknown"}\", PostDto: {(request.Muscle !=null ? request.Muscle.Print() : "null")} }}";
    }

    public static string Print(this MusclePostDTO request)
    {
        if (request == null)
        {
            return "MusclePostDTO <null>";
        }

        var antagonists = request.AntagonistIds is { Count: > 0 }
            ? string.Join(", ", request.AntagonistIds)
            : "none";

        var descriptor = request.Descriptor is null
            ? "null"
            : $"{{ Id: {request.Descriptor.Id}, DescriptionContent: \"{request.Descriptor.DescriptionContent}\", Notes: \"{request.Descriptor.Notes ?? "null"}\", Tags: [{(request.Descriptor.Tags is { Count: > 0 } tags ? string.Join(", ", tags) : "none")}], Authority: {request.Descriptor.Authority}, CreatedBy: {request.Descriptor.CreatedBy ?? "null"} }}";

        return
            $"MusclePostDTO {{ Id: {request.Id}, Name: \"{request.Name}\", BodySection: {request.BodySection}, AntagonistIds: [{antagonists}], Descriptor: {descriptor}, Authority: {request.Authority}, CreatedBy: {request.CreatedBy ?? "null"} }}";
    }

    public static MuscleCreateRequest ToCreateRequest(this MuscleCreateDTO dto)
    {
        return new MuscleCreateRequest(dto.ToPostDTO(), dto.CreateRequest, dto.FilledBy ?? "unknown");
    }

   
    
    public static MusclePostDTO ToPostDTO(this MuscleCreateDTO dto)
    {
        
        var descriptor = dto.Descriptor is DescriptorCreateDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorCreateDTO();

        
        return new MusclePostDTO
        {
            Id = dto.Id,
            Name = dto.Name?? "",
            BodySection = dto.BodySection,
            AntagonistIds = dto.Antagonists !=null ? dto.Antagonists.Select(a=>a.Id).ToList() :  null,
            Authority = dto.Authority ?? DataAuthority.Bidirectional,
            CreatedBy = dto.FilledBy,
            Descriptor = descriptor.ToPostDTO()
        };
    }
    
}
