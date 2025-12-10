using GainsLab.Domain.Interfaces;

namespace GainsLab.Domain.Entities.CreationInfo;

/// <summary>
/// Represents a full audit trail with creation, update, deletion and version metadata.
/// </summary>
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
    /// <summary>
    /// Creates an <see cref="AuditedInfo"/> initialized with creation details.
    /// </summary>
    public static AuditedInfo New( DateTimeOffset time, string name) =>
        new(time, name);

    /// <summary>
    /// Returns a copy with the update metadata refreshed to the supplied values.
    /// </summary>
    public AuditedInfo Touch( DateTimeOffset time, string updatedBy) =>
        this with { UpdatedAtUtc = time, UpdatedBy = updatedBy, Version = Version + 1 };
}

/// <summary>
/// Minimal audit metadata for entities that do not track creators.
/// </summary>
public sealed record BaseAuditedInfo(
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset? UpdatedAtUtc = null,
    string? UpdatedBy = null,
    long Version = 0,
    bool IsDeleted = false,
    DateTimeOffset? DeletedAt = null
)
{
   


    /// <summary>
    /// Captures the current clock time as creation metadata.
    /// </summary>
    public static BaseAuditedInfo New(IClock clock) =>
        new(clock.UtcNow);

    /// <summary>
    /// Returns a copy with the update metadata refreshed to the current clock time.
    /// </summary>
    public BaseAuditedInfo Touch(string updatedBy, IClock clock) =>
        this with { UpdatedAtUtc = clock.UtcNow, UpdatedBy = updatedBy, Version = Version + 1 };

    
    /// <summary>
    /// Creates metadata with a specific creation time.
    /// </summary>
    public static BaseAuditedInfo New( DateTimeOffset time) =>
        new(time);

    /// <summary>
    /// Returns a copy with update metadata populated from the provided timestamp.
    /// </summary>
    public BaseAuditedInfo Touch( DateTimeOffset time,string updatedBy) =>
        this with { UpdatedAtUtc = time, UpdatedBy = updatedBy, Version = Version + 1 };

    /// <summary>
    /// Generates a shallow copy of the metadata.
    /// </summary>
    public BaseAuditedInfo Copy()
    {
        return new BaseAuditedInfo(CreatedAtUtc, UpdatedAtUtc, UpdatedBy, Version);
    }
}
