using System;
using GainsLab.Models.Core.Descriptor;

namespace GainsLab.Models.Core.User;

public class User : IEquatable<User>
{
    
    public User(string name, UserRole role )
    {
        Identifier = new UserIdentifier(-1, name);
        CreationInfo = new  NewUserCreationInfo();
        Role =role;
    }

    //define the role of the user and its permissions- admin , coach or normal user 
    public UserRole Role { get; set; }
    
    //id for the db or cache
    public UserIdentifier Identifier { get; set; }

    //infos on the creation of the object - dates etc
    public UserCreationInfo CreationInfo { get; set; }
    
    
    //will need a user infos for metrics, ages etc 
    
    public int Id
    {
        get => Identifier.ID ?? -1;

        set => Identifier.ID = value;
    }
    
    public string Name
    {
        get => Identifier.Name ?? "Unknown user Name";

        set => Identifier.Name = value;
    }
    
    public Guid GUID
    {
        get => Identifier.GUID;

        set => Identifier.GUID = value;
    }
    
    public void Rename(string newName) => Name = newName;
    public void Promote(UserRole newRole) => Role = newRole;
    internal void SetDbId(int id) => Id = id; 

    
    public bool Equals(User? other) => other is not null && GUID == other.GUID;
    public override bool Equals(object? obj) => obj is User u && Equals(u);
    public override int GetHashCode() => GUID.GetHashCode();

    public override string ToString() => $"{Name} ({Role.Role}) [{GUID}]";
    
}

public class Coach(string name) : User(name, new UserRole.CoachRole());
public class  Athlete(string name) : User(name, new UserRole.AthleteRole());
public class Admin(string name) : User(name, new UserRole.AdminRole());