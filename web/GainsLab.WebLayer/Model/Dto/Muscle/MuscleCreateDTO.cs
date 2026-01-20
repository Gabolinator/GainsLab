using System.ComponentModel.DataAnnotations;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Domain;
using GainsLab.WebLayer.Model.Dto.Descriptor;

namespace GainsLab.WebLayer.Model.Dto.Muscle;

public class MuscleCreateDTO : MuscleFormDTO
{
    
    public override FormType FormType => FormType.Edit;
    
    public string? CreateBy => FilledBy;
    
    public CreateRequest CreateRequest =>  
        ApplyRequest == Request.ApplyRequest ? CreateRequest.Create :CreateRequest.DontCreate;

    
    public override string ToString()
    {
        return $"{Id.ToString()} -  {Name} - Antagonists : {string.Join(',', Antagonists)}";
    }
}