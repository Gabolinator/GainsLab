using System;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.Core;

public class WorkoutComponentIdentifier : Identifier
{
    public WorkoutComponentIdentifier() { } 
    
    public WorkoutComponentIdentifier(int? id, string? slug, eWorkoutComponents type) : base(id, slug)
    {
        ComponentType = type;
    }
    public WorkoutComponentIdentifier(string slug , eWorkoutComponents type): base(null, slug)
    {
        ComponentType = type;
    }

    
    public eWorkoutComponents ComponentType { get; set; }
    
    
    public override string ToString()
    {
        return $" Id : {(IsIdSet() ? DbID : "null")} , Uid : {(IsUidSet() ? Slug : "null")} ";
        
    }

    public override bool Equals(object? obj)
    {
        return obj is  WorkoutComponentIdentifier identifier && Equals(identifier);
    }

    public override bool Equals(IIdentifier other)
    {
        if (other is not WorkoutComponentIdentifier identifier) return false;
        return string.Equals(identifier.Slug, identifier.Slug, StringComparison.InvariantCultureIgnoreCase) && ComponentType == identifier.ComponentType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Slug, (int)ComponentType);
    }

    public override IIdentifier Copy()
    {
        return new WorkoutComponentIdentifier(DbID, Slug, ComponentType);
    }
}

public class EmptyWorkoutComponentIdentifier() : WorkoutComponentIdentifier(null, "Empty", eWorkoutComponents.unidentified);

