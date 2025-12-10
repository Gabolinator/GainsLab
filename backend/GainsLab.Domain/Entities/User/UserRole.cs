namespace GainsLab.Domain.Entities.User;

/// <summary>
/// Defines the primary role assigned to a GainsLab account.
/// </summary>
public enum AccountRole
{
    Admin =1,
    Coach =2,
    Athlete =3,
}

/// <summary>
/// Permission flags controlling access to operations across the platform.
/// </summary>
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

/// <summary>
/// Associates a role with its effective permissions.
/// </summary>
public class UserRole
{

    public UserRole(AccountRole role,   Permission permissions)
    {
        Role = role;
        Permissions = permissions;
    }
    

    public AccountRole Role { get; set; }
    public Permission Permissions { get; }

    /// <summary>
    /// Role preset granting full administrative permissions.
    /// </summary>
    public class AdminRole() : UserRole(AccountRole.Admin, Permission.All);
    /// <summary>
    /// Role preset granting permissions appropriate for athletes.
    /// </summary>
    public class AthleteRole() : UserRole(AccountRole.Athlete, Permission.ViewOwn | Permission.EditOwn);
    /// <summary>
    /// Role preset granting permissions appropriate for coaches.
    /// </summary>
    public class CoachRole() : UserRole(AccountRole.Coach, Permission.ViewOwn | Permission.EditOwn |
                                                               Permission.ViewOthers | Permission.EditOthers |
                                                               Permission.ManagePrograms | Permission.ViewAnalytics);

}

