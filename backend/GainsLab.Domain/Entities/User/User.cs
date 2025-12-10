using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.User;



/// <summary>
/// Aggregate root representing a GainsLab user with content, audit info, and account state.
/// </summary>
public class User : EntityBase<UserId,UserContent, BaseAuditedInfo> , IEquatable<User>
{
    
    public User(string name, AccountInfos role )
    {
        Id = UserId.New();
        Content = new UserContent(name);
        CreationInfo = new  BaseAuditedInfo(DateTimeOffset.UtcNow);
        Role =role;
    }
    
    public User(UserContent content, AccountInfos role )
     {
         Id = UserId.New();
         Content = content;
         CreationInfo = new  BaseAuditedInfo(DateTimeOffset.UtcNow);
         Role =role;
     }
    
    public User(UserContent content, AccountInfos role,  UserId id)
    {
        Id = id;
        Content = content;
        Role =role;
    }
    
    public User(UserContent content, AccountInfos role, UserId id, BaseAuditedInfo  infos)
    {
        Id = id;
        Content = content;
        CreationInfo = infos;
        Role =role;
    }
    
    public override EntityType Type => EntityType.User;
    
    public UserId Id { get; }
    
    public AccountInfos Role { get; set; }

    public IClock? Clock { get; set; } = default;

    public User Rename(string newName)
    {
        BaseAuditedInfo info = CreationInfo.Copy();
        info.Touch("rename" ,Clock);
        
        return new User(new UserContent(newName), Role ,Id,info );
        
    }
     public void Promote(UserRole newRole) => Role.Promote(newRole);
     public void Upgrade(SubscriptionInfos newSubscriptionInfos) => Role.Upgrade(newSubscriptionInfos);
     
    
     public bool Equals(User? other) => other is not null && Equals(other.Id, Id);
     public override bool Equals(object? obj) => obj is User u && Equals(u);
     public override int GetHashCode() => Content.GetHashCode();

     public string Name =>Content.PseudoName;
    
    
     public override string ToString() => $"{Name} ({Role.Role}) [{Id}]";

     

}
/// <summary>
/// Convenience type for creating coach accounts with default permissions.
/// </summary>
public class Coach(string name) : User(name, new AccountInfos(new UserRole.CoachRole(), new SubscriptionInfos.Free()));
/// <summary>
/// Convenience type for creating athlete accounts with default permissions.
/// </summary>
public class  Athlete(string name) : User(name, new AccountInfos(new UserRole.AthleteRole(),new SubscriptionInfos.Free()));
/// <summary>
/// Convenience type for creating admin accounts with default permissions.
/// </summary>
public class Admin(string name) : User(name, new AccountInfos(new UserRole.AdminRole(),new SubscriptionInfos.Paid()));
