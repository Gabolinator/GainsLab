using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Request;
using GainsLab.Domain;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Muscle;

public class MuscleEditFormDTO : MuscleFormDTO
{

    
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorEditDTO();
    public override FormType FormType => FormType.Edit;
 
    public string? UpdatedBy => FilledBy;

    public UpdateRequest UpdateRequest =>
        ApplyRequest  == Request.ApplyRequest ? UpdateRequest.Update : UpdateRequest.DontUpdate;

   

    public override string ToString()
    {
        return $"{Id.ToString()} -  {Name} - Antagonists : {string.Join(',', Antagonists)}";
    }
}

public static class MuscleEditFormDTOExtensions
{


    public static MuscleEditFormDTO FromGetDTO(this MuscleGetDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MuscleEditFormDTO
        {

            Id = dto.Id,
            Name = dto.Name,
            BodySection = dto.BodySection,
            Antagonists = dto.Antagonists?.ToList(),
            Authority = dto.Authority,
            Descriptor = dto.Descriptor is null
                ? new DescriptorEditDTO()
                : dto.Descriptor.FromGetDTO()
        };
    }

    public static MuscleUpdateDTO ToUpdateDTO(this MuscleEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        var descriptor = dto.Descriptor is DescriptorEditDTO descriptorEditDto ?  descriptorEditDto : new DescriptorEditDTO();
        return new MuscleUpdateDTO
        {
            BodySection = dto.BodySection,
            AntagonistIds = dto.Antagonists == null || !dto.Antagonists.Any()
                ? null
                : dto.Antagonists.Select(m => m.Id).ToList(),
            Name = dto.Name,
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy,
            Descriptor = descriptor.ToUpdateDTO() 
        };
    }
    


    public static MuscleUpdateRequest ToUpdateRequest(this MuscleEditFormDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new MuscleUpdateRequest(
            dto.ToUpdateDTO(),
            dto.Id,
            dto.UpdateRequest,
            dto.UpdatedBy ?? "unknown");

    }



     public static Result IsValid(this MuscleEditFormDTO muscle, Domain.Interfaces.ILogger? logger = null)
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

    




    
    public static string Print(this MuscleUpdateRequest request)
    {
        if (request == null)
        {
            return "MuscleUpdateRequest <null>";
        }

        return
            $"MuscleUpdateRequest {{ Request: {request.UpdateRequest}, FilledBy: \"{request.RequestedBy ?? "unknown"}\", PostDto: {(request.Muscle !=null ? request.Muscle.Print() : "null")} }}";
    }

    public static string Print(this  MuscleUpdateDTO request)
    {
        if (request == null)
        {
            return "MuscleUpdateDTO <null>";
        }

        var antagonists = request.AntagonistIds is { Count: > 0 }
            ? string.Join(", ", request.AntagonistIds)
            : "none";

        var descriptor = request.Descriptor is null
            ? "Update"
            : $"{{\"{request.Descriptor.DescriptionContent}\", Notes: \"{request.Descriptor.Notes ?? "null"}\", Tags: [{(request.Descriptor.Tags is { Count: > 0 } tags ? string.Join(", ", tags) : "none")}], Authority: {request.Descriptor.Authority}, CreatedBy: {request.Descriptor.UpdatedBy ?? "null"} }}";

        return
            $"MuscleUpdateDTO {{Name: \"{request.Name}\", BodySection: {request.BodySection}, AntagonistIds: [{antagonists}], Descriptor: {descriptor}, Authority: {request.Authority}, CreatedBy: {request.UpdatedBy ?? "null"} }}"; }

 
   
    
    public static MuscleUpdateDTO ToUpdateDto(this MuscleEditFormDTO dto)
    {
        
        var descriptor = dto.Descriptor is DescriptorEditDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorEditDTO();

        
        return new  MuscleUpdateDTO
        {
            Name = dto.Name?? "",
            BodySection = dto.BodySection,
            AntagonistIds = dto.Antagonists !=null ? dto.Antagonists.Select(a=>a.Id).ToList() :  null,
            Authority = dto.Authority ?? DataAuthority.Bidirectional,
            UpdatedBy = dto.FilledBy,
            Descriptor = descriptor.ToUpdateDTO()
        };
    }
    
    
}