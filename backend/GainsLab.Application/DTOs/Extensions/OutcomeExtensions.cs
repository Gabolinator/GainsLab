using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto.Outcome;

namespace GainsLab.Application.DTOs.Extensions;

public static partial class EquipmentCreateOutcomeExtensions
{
    public static Result IsCreated(this EquipmentCreateOutcome? outcome)
    {
        //return !(outcome== null  || outcome.CreatedEquipment == null || outcome.Outcome!= CreateOutcome.Created);
        
      return Result.Failure("Not Implemented");
    }
    
}


public static partial class DescriptorCreateOutcomeExtensions
{
    public static Result IsCreated(this DescriptorCreateOutcome? outcome)
    {
       // return !(outcome== null  || outcome.CreatedDescriptor == null || outcome.Outcome!= CreateOutcome.Created);
        return Result.Failure("Not Implemented");
    }
}