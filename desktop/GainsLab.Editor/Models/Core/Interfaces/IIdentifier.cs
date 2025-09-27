using System;

namespace GainsLab.Models.Core.Interfaces;

public interface IIdentifier
{
    int? ID { get; set; } // for database primary key
    string? UID { get; set; } // Public-friendly or shareable (e.g., "pushup-001")

    Guid GUID { get; set; }

    public bool IsIdSet();

    public bool IsEmpty();

    string ToString();

    bool Equals(object? obj);
    
    bool Equals(IIdentifier obj);
    int GetHashCode();


    IIdentifier Copy();
}