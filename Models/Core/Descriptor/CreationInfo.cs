using System;

namespace GainsLab.Models.Core;

public record CreationInfo(string? CreatedBy, Identifier Identifier ,DateTime? CreatedAt, DateTime? UpdatedAt = null)
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(CreatedBy) && Identifier.IsEmpty();

    public CreationInfo(): this("", new EmptyIdentifier(), null, null)
    {
        
    }

    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public CreationInfo Copy()
    {
        return new CreationInfo(CreatedBy, Identifier, CreatedAt, UpdatedAt);
    }
    
    public override string ToString()
    {
        var created = CreatedAt?.ToString("s") ?? "unknown";
        var updated = UpdatedAt?.ToString("s") ?? "never";
        return $"Created by: {CreatedBy ?? "unknown"}, At: {created}, Updated: {updated}"
               + (Identifier != null ? $", ID: {Identifier}" : "");
    }
}

public record EmptyCreationInfo() : CreationInfo(null, null, null, null)
{
    public override string ToString() => "CreationInfo: (empty)";
}

public record DefaultCreationInfo(eWorkoutComponents type) 
    : CreationInfo("system", new Identifier("system", type), DateTime.Now)
{
    public override string ToString() => base.ToString() + " (default)";
}