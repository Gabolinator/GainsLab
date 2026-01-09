using System.ComponentModel.DataAnnotations;
using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PostDto.Request;
using GainsLab.Infrastructure.Utilities;
using GainsLab.WebLayer.Model.Descriptor;
using ILogger = GainsLab.Domain.Interfaces.ILogger;


namespace GainsLab.WebLayer.Model.Equipment;

public class EquipmentCreateDTO
{
    public Guid  Id { get; init; } = Guid.NewGuid();
    
    [StringLength(256, MinimumLength = 2), Required]
    public string? Name { get; set; } = "new name";

    public DescriptorCreateDTO? Descriptor { get; set; } = new DescriptorCreateDTO();
    
    public string? CreatedBy { get; set; }
    
    public CreateRequest CreateRequest { get; set; } = CreateRequest.Create;
    
   
}


public static class EquipmentCreateDTOExtensions
{

    public static EquipmentCombineCreateRequest ToCombineCreateRequest(this EquipmentCreateDTO dto)
    {
        return new EquipmentCombineCreateRequest(dto.ToCreateRequest(), dto.Descriptor.ToCreateRequest());
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
        return new EquipmentPostDTO
        {
            Id = dto.Id,
            Name = dto.Name,
            CreatedBy = dto.CreatedBy,
            Descriptor = dto.Descriptor.ToPostDTO()
        };
    }
}