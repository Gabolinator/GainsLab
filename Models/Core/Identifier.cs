using System;

namespace GainsLab.Models.Core;

public class Identifier : IIdentifier
{
    
    
    public Identifier(int? id, string? uid, eWorkoutComponents type)
    {
        ID = id;
        UID = uid;
        ComponentType = type;
    }
    public Identifier(string uid , eWorkoutComponents type)
    {
        ID = null;
        UID = uid;
        ComponentType = type;
    }


    public int? ID { get; set; } = -1;
    public string? UID { get; set; }
    
    public bool IsIdSet() => ID != null || ID <0;

    public bool IsUidSet() => !string.IsNullOrEmpty(UID);

    public bool IsEmpty() => !IsIdSet() && !IsUidSet();
    
    public eWorkoutComponents ComponentType { get; set; }

    public override string ToString()
    {
        return $" Id : {(IsIdSet() ? ID : "null")} , Uid : {(IsUidSet() ? UID : "null")} ";
        
    }

    public override bool Equals(object? obj)
    {
        return obj is IIdentifier identifier && Equals(identifier);
    }

    public bool Equals(IIdentifier other)
    {
        return string.Equals(other.UID, other.UID, StringComparison.InvariantCultureIgnoreCase) && ComponentType == other.ComponentType;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UID, (int)ComponentType);
    }

    public IIdentifier Copy()
    {
        return new Identifier(ID, UID, ComponentType);
    }
}

public class EmptyIdentifier() : Identifier(null, "Empty", eWorkoutComponents.unidentified);

