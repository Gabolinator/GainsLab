using System;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Models.Core;


//used only for data base - and general object identification
public class Identifier : IIdentifier
{
    public Identifier() { } 
    
    public Identifier(int? id, string? slug)
    {
        DbID = id;
        Slug = slug;
        GUID = Guid.NewGuid();
    }
    public Identifier(string slug)
    {
        DbID = null;
        Slug = slug;
        GUID = Guid.NewGuid();
    }


    public int? DbID { get; set; } = -1;
    public string? Slug { get; set; }
    public Guid GUID { get; set; } = Guid.Empty;


    public bool IsIdSet() => DbID != null || DbID > 0;

    public bool IsUidSet() => !string.IsNullOrEmpty(Slug);
    
    public bool IsGuidSet() => GUID != Guid.Empty;

    public bool IsEmpty() => !IsIdSet() && !IsUidSet() && !IsGuidSet();
    

    public override string ToString()
    {
        return $" Id : {(IsIdSet() ? DbID : "null")} , Uid : {(IsUidSet() ? Slug : "null")} ";
        
    }

    public override bool Equals(object? obj)
    {
        return obj is  Identifier identifier && Equals(identifier);
    }

    public virtual bool Equals(IIdentifier other)
    {
        if (other is not Identifier identifier) return false;
        return string.Equals(identifier.Slug, identifier.Slug, StringComparison.InvariantCultureIgnoreCase);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Slug);
    }

    public virtual IIdentifier Copy()
    {
        return new Identifier(DbID, Slug);
    }

    public void WithDBId(int id) =>DbID = id;
    
    public void AssignNewGuid() =>GUID = new Guid();
    
}


