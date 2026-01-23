using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Domain;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Dto.Descriptor;
using ILogger = GainsLab.Domain.Interfaces.ILogger;


namespace GainsLab.WebLayer.Model.Dto.Equipment;

public class EquipmentCreateDTO : EquipmentFormDTO
{
  
    public override DescriptorFormDTO Descriptor { get; set; } = new DescriptorCreateDTO();
    public override FormType FormType => FormType.Edit;
 
    public string? CreatedBy => FilledBy;

    public CreateRequest CreateRequest =>
        ApplyRequest  == Request.ApplyRequest ? CreateRequest.Create : CreateRequest.DontCreate;
    
   
}


public static class EquipmentCreateDTOExtensions
{

    public static EquipmentCombineCreateRequest ToCombineCreateRequest(this EquipmentCreateDTO dto)
    {
        var descriptor = dto.Descriptor is DescriptorCreateDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorCreateDTO();

        
        return new EquipmentCombineCreateRequest(dto.ToCreateRequest(), descriptor.ToCreateRequest());
    }

    public static Result IsValid(this EquipmentCreateDTO dto, ILogger? logger = null)
    {
        if(dto.Id ==  Guid.Empty) return Result.Failure("Invalid Id");
        var name = StringFormater.RemoveUnwantedChar(dto.Name);
        var comparison = "new name";
        var equals = string.Equals(
            StringFormater.RemoveUnwantedChar(dto.Name),
            StringFormater.RemoveUnwantedChar(comparison),
            StringComparison.InvariantCultureIgnoreCase);
        
        if(logger != null)logger.Log("EquipmentCreateDTO.IsValid()", $"name {name} - comparison :{comparison} - equals: {equals}");
        
        if (string.IsNullOrEmpty(dto.Name) || equals)
            return Result.Failure("Invalid Name");

        return Result.SuccessResult();
        
    }

    public static EquipmentCreateRequest ToCreateRequest(this EquipmentCreateDTO dto)
    {
      return  new EquipmentCreateRequest(dto.ToPostDTO(), dto.CreateRequest,dto.CreatedBy?? "unknown" );
    }
    
    public static EquipmentPostDTO ToPostDTO(this EquipmentCreateDTO dto)
    {
        var descriptor = dto.Descriptor is DescriptorCreateDTO descriptorEditDto ?  descriptorEditDto : new  DescriptorCreateDTO();

        
        return new EquipmentPostDTO
        {
            Id = dto.Id,
            Name = dto.Name,
            CreatedBy = dto.CreatedBy,
            Descriptor = descriptor.ToPostDTO()
        };
    }
}