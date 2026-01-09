namespace GainsLab.Contracts.Dtos.ID;

public record EntityId(
    Guid? Id,
    string? Name //by name not yet supported 
)
{
    public override string ToString() => $"{Id}:{Name}";
    public bool IsIdValid()  => Id is not null && Id.Value != Guid.Empty;
    public bool IsNameValid() => !string.IsNullOrWhiteSpace(Name);
    
    public bool IsValid() => IsIdValid() || IsNameValid();
};


public record EquipmentEntityId(
    Guid? Id,
    string? Name = null   //by name not yet supported 
) :EntityId(Id, Name);