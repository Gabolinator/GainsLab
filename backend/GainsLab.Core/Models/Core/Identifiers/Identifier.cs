using System;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.Core;


//used only for data base - and general object identification
public class Identifier : IIdentifier
{
    public Identifier() { } 
    
    public Identifier(int? id, string? uid)
    {
        ID = id;
        UID = uid;
        GUID = Guid.NewGuid();
    }
    public Identifier(string uid)
    {
        ID = null;
        UID = uid;
        GUID = Guid.NewGuid();
    }


    public int? ID { get; set; } = -1;
    public string? UID { get; set; }
    public Guid GUID { get; set; } = Guid.Empty;


    public bool IsIdSet() => ID != null || ID > 0;

    public bool IsUidSet() => !string.IsNullOrEmpty(UID);
    
    public bool IsGuidSet() => GUID != Guid.Empty;

    public bool IsEmpty() => !IsIdSet() && !IsUidSet() && !IsGuidSet();
    

    public override string ToString()
    {
        return $" Id : {(IsIdSet() ? ID : "null")} , Uid : {(IsUidSet() ? UID : "null")} ";
        
    }

    public override bool Equals(object? obj)
    {
        return obj is  Identifier identifier && Equals(identifier);
    }

    public virtual bool Equals(IIdentifier other)
    {
        if (other is not Identifier identifier) return false;
        return string.Equals(identifier.UID, identifier.UID, StringComparison.InvariantCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(UID);
    }

    public virtual IIdentifier Copy()
    {
        return new Identifier(ID, UID);
    }
}

public class EmptyIdentifier() : Identifier(null, "Empty");

