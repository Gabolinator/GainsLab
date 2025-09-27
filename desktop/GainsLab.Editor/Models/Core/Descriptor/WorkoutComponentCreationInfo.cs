using System;

namespace GainsLab.Models.Core.Descriptor;

public record WorkoutComponentCreationInfo(
    string? CreatedBy,
    Identifier Identifier,
    DateTime? CreatedAt,
    DateTime? UpdatedAt = null) : CreationInfo(CreatedBy,Identifier, CreatedAt, UpdatedAt )
{
    public bool IsEmpty() => string.IsNullOrWhiteSpace(CreatedBy) && Identifier.IsEmpty();

    public WorkoutComponentCreationInfo(): this("", new EmptyWorkoutComponentIdentifier(), null, null)
    {
        
    }

    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public WorkoutComponentCreationInfo Copy()
    {
        return new WorkoutComponentCreationInfo(CreatedBy, Identifier, CreatedAt, UpdatedAt);
    }
    
    public override string ToString()
    {
        var created = CreatedAt?.ToString("s") ?? "unknown";
        var updated = UpdatedAt?.ToString("s") ?? "never";
        return $"Created by: {CreatedBy ?? "unknown"}, At: {created}, Updated: {updated}"
               + (Identifier != null ? $", ID: {Identifier}" : "");
    }
}

public record EmptyWorkoutComponentCreationInfo() : WorkoutComponentCreationInfo(null, null, null, null)
{
    public override string ToString() => "CreationInfo: (empty)";
}

public record DefaultWorkoutComponentCreationInfo(eWorkoutComponents type) 
    : WorkoutComponentCreationInfo("system", new WorkoutComponentIdentifier("system", type), DateTime.Now)
{
    public override string ToString() => base.ToString() + " (default)";
}