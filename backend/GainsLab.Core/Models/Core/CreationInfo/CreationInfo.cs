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
    public static AuditedInfo New(string name) =>
        new(DateTimeOffset.UtcNow, name);

    public AuditedInfo Touch(string updatedBy) =>
        this with { UpdatedAtUtc = DateTimeOffset.UtcNow, UpdatedBy = updatedBy, Version = Version + 1 };
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
    public static BaseAuditedInfo New() =>
        new(DateTimeOffset.UtcNow);

    public BaseAuditedInfo Touch(string updatedBy) =>
        this with { UpdatedAtUtc = DateTimeOffset.UtcNow, UpdatedBy = updatedBy, Version = Version + 1 };

    public BaseAuditedInfo Copy()
    {
        return new BaseAuditedInfo(CreatedAtUtc, UpdatedAtUtc, UpdatedBy, Version);
    }
}
