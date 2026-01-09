using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.DTOs;

public static class DescriptorUpdateOutcomeExtensions
{
    public static MessagesContainer GetOutcomeMessages(this DescriptorUpdateOutcome? descriptor)
    {
        
        var messages = new MessagesContainer();
            
        if (descriptor == null)
        {
            messages.AddError($"Descriptor null - Not Updated");
            return  messages;
        }

        switch (descriptor.Outcome)
        {
            case UpdateOutcome.NotUpdated or UpdateOutcome.NotRequested:
                messages.AddInfo(
                    $"Descriptor {descriptor!.UpdatedState!.Id} Not Updated {descriptor.Outcome}");
                break;
            case UpdateOutcome.Updated:
                messages.AddInfo($"Descriptor {descriptor!.UpdatedState!.Id} Updated to {descriptor.UpdatedState!.content}");
                break;
            case UpdateOutcome.Failed: 
                messages.AddError(
                    $"Failed to Update Descriptor {descriptor!.UpdatedState!.Id}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return  messages;
    }
}

public static class EquipmentUpdateOutcomeExtensions
{
   
    
    public static MessagesContainer  GetOutcomeMessages (this EquipmentUpdateOutcome? equipment)
    {
        var messages = new MessagesContainer();
        
        if (equipment == null)
        {
            messages.AddError($"Failed to Update Equipment");
        }

        
        else  switch (equipment.Outcome)
        {
            case UpdateOutcome.NotUpdated or UpdateOutcome.NotRequested:
                messages.AddInfo($"Equipment {equipment.UpdatedState!.Name} Not Updated");
                break;
            case UpdateOutcome.Updated:
                messages.AddInfo($"Equipment Updated to {equipment.UpdatedState!.Name}");
                break;
            case UpdateOutcome.Failed:
                messages.AddError($"Failed to Update Equipment");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return messages;
    }
    
}


public static class EquipmentCombinedOutcomeExtension
{
    public static MessagesContainer GetOutcomeMessages (this EquipmentUpdateCombinedOutcome equipmentUpdate,
        MessagesContainer? messages =null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentUpdate.Equipment.GetOutcomeMessages());
        messages.Append(equipmentUpdate.DescriptorUpdateOutcome.GetOutcomeMessages());
        
        return  messages;
       
    }

}

public static partial class DescriptorCreateOutcomeExtensions
{
    public static MessagesContainer GetOutcomeMessages(this DescriptorCreateOutcome? descriptor)
    {
        
        var messages = new MessagesContainer();
            
        if (descriptor == null)
        {
            messages.AddError($"Descriptor null - Not Updated");
            return  messages;
        }

        switch (descriptor.Outcome)
        {
            case CreateOutcome.AlreadyExist or CreateOutcome.Canceled:
                messages.AddInfo(
                    $"Descriptor {descriptor!.CreatedDescriptor!.Id} Not Created {descriptor.Outcome}");
                break;
            case CreateOutcome.Created:
                messages.AddInfo($"Descriptor {descriptor!.CreatedDescriptor!.Id} Created to {descriptor.CreatedDescriptor!.content}");
                break;
            case CreateOutcome.Failed: 
                messages.AddError(
                    $"Failed to Update Descriptor {descriptor!.CreatedDescriptor!.Id}");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return  messages;
    }
}

public static  partial class EquipmentCreateOutcomeExtensions
{
   
   
    public static MessagesContainer  GetOutcomeMessages (this EquipmentCreateOutcome? equipment)
    {
        var messages = new MessagesContainer();
        
        if (equipment == null)
        {
            messages.AddError($"Failed to Update Equipment");
        }

        
        else  switch (equipment.Outcome)
        {
            case CreateOutcome.AlreadyExist or CreateOutcome.Canceled:
                messages.AddInfo($"Equipment {equipment.CreatedEquipment!.Name} Not Updated");
                break;
            case CreateOutcome.Created:
                messages.AddInfo($"Equipment Updated to {equipment.CreatedEquipment!.Name}");
                break;
            case CreateOutcome.Failed:
                messages.AddError($"Failed to Update Equipment");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return messages;
    }
    
}


public static class EquipmentCreateCombinedOutcomeExtension
{
    
    public static MessagesContainer GetOutcomeMessages (this EquipmentCreateCombineOutcome equipmentCreated,
        MessagesContainer? messages =null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentCreated.CreatedEquipmentOutcome.GetOutcomeMessages());
        messages.Append(equipmentCreated.CreatedDescriptorOutcome.GetOutcomeMessages());
        
        return  messages;
       
    }

}