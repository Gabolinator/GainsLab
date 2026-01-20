using GainsLab.Application.Results;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.PostDto.Outcome;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;

namespace GainsLab.Application.DTOs;

#region Update

#region Muscle

public static class MuscleUpdateOutcomeExtensions
{
    public static MessagesContainer GetOutcomeMessages (this MuscleUpdateCombinedOutcome equipmentUpdate,
        MessagesContainer? messages =null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentUpdate.Muscle.GetOutcomeMessages());
        messages.Append(equipmentUpdate.Descriptor.GetOutcomeMessages());
        
        return  messages;
    }

    public static MessagesContainer GetOutcomeMessages(this MuscleUpdateOutcome? muscle)
    {
        var messages = new MessagesContainer();

        if (muscle == null)
        {
            messages.AddError($"Failed to Update Movement Category");
        }


        else
            switch (muscle.Outcome)
            {
                case UpdateOutcome.NotUpdated or UpdateOutcome.NotRequested:
                    messages.AddInfo($"Movement Category {muscle.UpdatedState!.Name} Not Updated");
                    break;
                case UpdateOutcome.Updated:
                    messages.AddInfo($"Movement Category Updated to {muscle.UpdatedState!.Name}");
                    break;
                case UpdateOutcome.Failed:
                    messages.AddError($"Failed to Update Movement Category");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        return messages;

    }
}

#endregion



#region Category

public static class MovementCategoryUpdateOutcomeExtension
{
    public static MessagesContainer GetOutcomeMessages(this MovementCategoryUpdateCombinedOutcome equipmentUpdate,
        MessagesContainer? messages = null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentUpdate.MovementCategory.GetOutcomeMessages());
        messages.Append(equipmentUpdate.Descriptor.GetOutcomeMessages());

        return messages;
    }


    public static MessagesContainer GetOutcomeMessages(this MovementCategoryUpdateOutcome? equipment)
    {
        var messages = new MessagesContainer();

        if (equipment == null)
        {
            messages.AddError($"Failed to Update Movement Category");
        }


        else
            switch (equipment.Outcome)
            {
                case UpdateOutcome.NotUpdated or UpdateOutcome.NotRequested:
                    messages.AddInfo($"Movement Category {equipment.UpdatedState!.Name} Not Updated");
                    break;
                case UpdateOutcome.Updated:
                    messages.AddInfo($"Movement Category Updated to {equipment.UpdatedState!.Name}");
                    break;
                case UpdateOutcome.Failed:
                    messages.AddError($"Failed to Update Movement Category");
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

        return messages;

    }
}

#endregion

#region Descriptor


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


#endregion

#region Equipment


public static class EquipmentOutcomeExtensions
{
    public static MessagesContainer GetOutcomeMessages (this EquipmentUpdateCombinedOutcome equipmentUpdate,
        MessagesContainer? messages =null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentUpdate.Equipment.GetOutcomeMessages());
        messages.Append(equipmentUpdate.Descriptor.GetOutcomeMessages());
        
        return  messages;
       
    }
    
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
#endregion

#endregion

#region Create

#region Category

public static class MovementCategoryOutcomeExtensions
{
    public static MessagesContainer GetOutcomeMessages (this MovementCategoryCreateCombineOutcome equipmentUpdate,
        MessagesContainer? messages =null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentUpdate.MovementCategory.GetOutcomeMessages());
        messages.Append(equipmentUpdate.Descriptor.GetOutcomeMessages());
        
        return  messages;
       
    }
    

    public static MessagesContainer GetOutcomeMessages (this MovementCategoryCreateOutcome? equipment)
    {
      
       var messages = new MessagesContainer();
        
        if (equipment == null)
        {
            messages.AddError($"Failed to Update MovementCategory");
        }

        
        else  switch (equipment.Outcome)
        {
            case CreateOutcome.AlreadyExist or CreateOutcome.Canceled:
                messages.AddInfo($"MovementCategory {equipment.CreatedMovementCategory!.Name} Not Updated");
                break;
            case CreateOutcome.Created:
                messages.AddInfo($"MovementCategoryt Updated to {equipment.CreatedMovementCategory!.Name}");
                break;
            case CreateOutcome.Failed:
                messages.AddError($"Failed to Update MovementCategory");
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return messages;
    }

}


#endregion

#region Descriptor

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


#endregion

#region Equipment

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
    

    
    
    public static MessagesContainer GetOutcomeMessages (this EquipmentCreateCombineOutcome equipmentCreated,
        MessagesContainer? messages =null)
    {
        messages ??= new MessagesContainer();

        messages.Append(equipmentCreated.CreatedEquipmentOutcome.GetOutcomeMessages());
        messages.Append(equipmentCreated.CreatedDescriptorOutcome.GetOutcomeMessages());
        
        return  messages;
       
    }
    
}

#endregion

#endregion
