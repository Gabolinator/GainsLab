using GainsLab.Models.Core.User;

namespace GainsLab.Core.Models.Core.Entities.User;

/// <summary>
/// Captures role and subscription information for a user account.
/// </summary>
public class AccountInfos
{
    public AccountInfos(UserRole role, SubscriptionInfos infos)
    {
        Role = role;
        Subscription = infos;
    }

    public UserRole Role { get; private set; }

    public SubscriptionInfos Subscription { get; private set; }

    public void Promote(UserRole newRole) => Role = newRole;

    public void Upgrade(SubscriptionInfos newSubscriptionInfos) => Subscription = newSubscriptionInfos;

}

/// <summary>
/// Stores the subscription tier assigned to a user.
/// </summary>
public class SubscriptionInfos
{
    public SubscriptionInfos(SubscriptionType type)
    {
        Type = type;
    }

    public enum SubscriptionType
    {
        Free,
        Paid,
        Trial
    }

    public SubscriptionType Type { get; private set; }
  
    public class Free() : SubscriptionInfos(SubscriptionType.Free);
    public class Trial() : SubscriptionInfos(SubscriptionType.Trial);
    public class Paid() : SubscriptionInfos(SubscriptionType.Paid);
    
    

}
