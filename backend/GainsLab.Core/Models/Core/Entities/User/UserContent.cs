using System;

namespace GainsLab.Models.Core.User;

/// <summary>
/// Immutable snapshot of personal information kept for a user.
/// </summary>
public sealed record UserContent(string PseudoName, DateTime? DateBirth= null, float? Weight = null, float?  Height = null) 
{
    
    public bool Equals(UserContent? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return PseudoName == other.PseudoName && Nullable.Equals(DateBirth, other.DateBirth) && Nullable.Equals(Weight, other.Weight) && Nullable.Equals(Height, other.Height);
    }
    

    public override int GetHashCode()
    {
        return HashCode.Combine(PseudoName, DateBirth, Weight, Height);
    }
}
