using System;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.Core;

public class WorkoutComponentIdentifier : Identifier
{
    public WorkoutComponentIdentifier() { } 
    
    public WorkoutComponentIdentifier(int? id, string? uid, eWorkoutComponents type) : base(id, uid)
    {
        ComponentType = type;
    }
    public WorkoutComponentIdentifier(string uid , eWorkoutComponents type): base(null, uid)
    {
        ComponentType = type;
    }

    
    public eWorkoutComponents ComponentType { get; set; }
    
    
    public override string ToString()
    {
        return $" Id : {(IsIdSet() ? ID : "null")} , Uid : {(IsUidSet() ? UID : "null")} ";
        
    }

    public override bool Equals(object? obj)
    {
        return obj is  WorkoutComponentIdentifier identifier && Equals(identifier);
    }

    public override bool Equals(IIdentifier other)
    {
        if (other is not WorkoutComponentIdentifier identifier) return false;
        return string.Equals(identifier.UID, identifier.UID, StringComparison.InvariantCultureIgnoreCase) && ComponentType == identifier.ComponentType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UID, (int)ComponentType);
    }

    public override IIdentifier Copy()
    {
        return new WorkoutComponentIdentifier(ID, UID, ComponentType);
    }
}

public class EmptyWorkoutComponentIdentifier() : WorkoutComponentIdentifier(null, "Empty", eWorkoutComponents.unidentified);

