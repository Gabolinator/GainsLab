using GainsLab.Models.Core.Interfaces;
using GainsLab.Models.Utilities;

namespace GainsLab.Core.Models.Core.CreationInfo;

public sealed record AuditedInfo(
    DateTimeOffset CreatedAtUtc,
    string CreatedBy,
    DateTimeOffset? UpdatedAtUtc = null,
    string? UpdatedBy = null,
    long Version = 0,
    bool IsDeleted = false,
    DateTimeOffset? DeletedAt = null,
    string? DeletedBy = null
) 
{
    
 
    
    public static AuditedInfo New( DateTimeOffset time, string name) =>
        new(time, name);

    public AuditedInfo Touch( DateTimeOffset time, string updatedBy) =>
        this with { UpdatedAtUtc = time, UpdatedBy = updatedBy, Version = Version + 1 };
}

public sealed record BaseAuditedInfo(
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc = null,
    string? UpdatedBy = null,
    long Version = 0,
    bool IsDeleted = false,
    DateTimeOffset? DeletedAt = null
)
{

    //todo - create 
    public static IClock Clock => CoreUtilities.Clock;


    public static BaseAuditedInfo New() =>
        new(Clock.UtcNow);

    public BaseAuditedInfo Touch(string updatedBy) =>
        this with { UpdatedAtUtc = Clock.UtcNow, UpdatedBy = updatedBy, Version = Version + 1 };

    
    public static BaseAuditedInfo New( DateTimeOffset time) =>
        new(time);

    public BaseAuditedInfo Touch( DateTimeOffset time,string updatedBy) =>
        this with { UpdatedAtUtc = time, UpdatedBy = updatedBy, Version = Version + 1 };

    public BaseAuditedInfo Copy()
    {
        return new BaseAuditedInfo(CreatedAtUtc, UpdatedAtUtc, UpdatedBy, Version);
    }
}
