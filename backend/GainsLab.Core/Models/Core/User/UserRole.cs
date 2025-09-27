using System;

namespace GainsLab.Models.Core.User;



public enum AccountRole
{
    Admin =1,
    Coach =2,
    Athlete =3,
}

[Flags]
public enum Permission
{
    None           = 0,
    ViewOwn        = 1 << 0,
    EditOwn        = 1 << 1,
    ViewOthers     = 1 << 2,
    EditOthers     = 1 << 3,
    ManageUsers    = 1 << 4,
    ManagePrograms = 1 << 5,
    ViewAnalytics  = 1 << 6,
    All            = ~0
}

public class UserRole
{

    public UserRole(AccountRole role,   Permission permissions)
    {
        Role = role;
        Permissions = permissions;
    }
    

    public AccountRole Role { get; set; }
    public Permission Permissions { get; }

    
    
    public class AdminRole() : UserRole(AccountRole.Admin, Permission.All);
    public class AthleteRole() : UserRole(AccountRole.Athlete, Permission.ViewOwn | Permission.EditOwn);
    public class CoachRole() : UserRole(AccountRole.Coach, Permission.ViewOwn | Permission.EditOwn |
                                                               Permission.ViewOthers | Permission.EditOthers |
                                                               Permission.ManagePrograms | Permission.ViewAnalytics);

}

