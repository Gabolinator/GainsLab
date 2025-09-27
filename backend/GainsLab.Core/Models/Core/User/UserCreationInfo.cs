using System;
using GainsLab.Models.Core.Descriptor;

namespace GainsLab.Models.Core.User;
public record UserCreationInfo(
    string? CreatedBy,
    Identifier Identifier,
    DateTime? CreatedAt,
    DateTime? UpdatedAt = null) : CreationInfo(CreatedBy,Identifier, CreatedAt, UpdatedAt )
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(CreatedBy) && Identifier.IsEmpty();

    public UserCreationInfo(): this("", new EmptyIdentifier(), null, null)
    {
        
    }

    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public UserCreationInfo Copy()
    {
        return new UserCreationInfo(CreatedBy, Identifier, CreatedAt, UpdatedAt);
    }
    
    public override string ToString()
    {
        var created = CreatedAt?.ToString("s") ?? "unknown";
        var updated = UpdatedAt?.ToString("s") ?? "never";
        return $"Created by: {CreatedBy ?? "unknown"}, At: {created}, Updated: {updated}"
               + (Identifier != null ? $", ID: {Identifier}" : "");
    }
}


public record NewUserCreationInfo() : UserCreationInfo("system", new Identifier(), DateTime.UtcNow, DateTime.UtcNow)
{
    public override string ToString() => "CreationInfo: (empty)";
}

public record EmptyUserCreationInfo() : UserCreationInfo(null, null, null, null)
{
    public override string ToString() => "CreationInfo: (empty)";
}

