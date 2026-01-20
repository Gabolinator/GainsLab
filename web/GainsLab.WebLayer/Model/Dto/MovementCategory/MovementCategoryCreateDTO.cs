using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Domain;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.MovementCategory;

public class MovementCategoryCreateDTO :  MovementCategoryFormDTO
{
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorCreateDTO();
    public override FormType FormType => FormType.Edit;
 
    public string? CreatedBy => FilledBy;

    public CreateRequest CreateRequest =>
        ApplyRequest  == Request.ApplyRequest ? CreateRequest.Create : CreateRequest.DontCreate;
    
    
    public override string ToString()
    {
        return $"{Id.ToString()} -  {Name} - Parent:  {(Parent != null ? Parent.Name : "none")} Bases : {string.Join(',', BasesCategory)}";
    }
    
    
    
}

public static class MovementCategoryCreateDTOExtensions
{

    public static MovementCategoryCombineCreateRequest ToCombineCreateRequest(this MovementCategoryCreateDTO dto)
    {
        var descriptor = dto.Descriptor is DescriptorCreateDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorCreateDTO();

        return new MovementCategoryCombineCreateRequest(dto.ToCreateRequest(), descriptor.ToCreateRequest());
    }

    public static Result IsValid(this MovementCategoryCreateDTO dto, Domain.Interfaces.ILogger? logger = null)
    {
        if (dto.Id == Guid.Empty) return Result.Failure("Invalid Id");
        var name = StringFormater.RemoveUnwantedChar(dto.Name);
        var comparison = "new name";
        var equals = string.Equals(
            StringFormater.RemoveUnwantedChar(dto.Name),
            StringFormater.RemoveUnwantedChar(comparison),
            StringComparison.InvariantCultureIgnoreCase);

        if (logger != null)
            logger.Log("MovementCategoryCreateDTO.IsValid()",
                $"name {name} - comparison :{comparison} - equals: {equals}");

        if (string.IsNullOrEmpty(dto.Name) || equals)
            return Result.Failure("Invalid Name");

        return Result.SuccessResult();

    }

    public static MovementCategoryCreateRequest ToCreateRequest(this MovementCategoryCreateDTO dto)
    {
        return new MovementCategoryCreateRequest(dto.ToPostDTO(), dto.CreateRequest, dto.CreatedBy ?? "unknown");
    }

    public static MovementCategoryPostDTO ToPostDTO(this MovementCategoryCreateDTO dto)
    {
        
        
        var basecat = dto.BasesCategory
            .Where(c => Enum.TryParse(c.Name, out eMovementCategories cat))
            .Select(c => Enum.Parse<eMovementCategories>(c.Name))
            .Where(c => c != eMovementCategories.undefined);
        
        var descriptor = dto.Descriptor is DescriptorCreateDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorCreateDTO();

        
        return new MovementCategoryPostDTO
        {
            Id = dto.Id,
            Name = dto.Name?? "",
            ParentCategoryId =  dto.Parent?.Id,
            BaseCategories = basecat.ToList(),
            CreatedBy = dto.CreatedBy,
            Descriptor = descriptor.ToPostDTO()
        };
    }
}