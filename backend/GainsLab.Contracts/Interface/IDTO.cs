using GainsLab.Domain;

namespace GainsLab.Contracts.Interface;

/// <summary>
/// Represents the minimal shape required for DTOs managed by the persistence layer.
/// </summary>
public interface IRecord
{
    /// <summary>
    /// Gets the database-assigned identifier for the DTO.
    /// </summary>
    int Iid { get; }

    /// <summary>
    /// Gets the stable GUID associated with the DTO.
    /// </summary>
    Guid  Iguid { get;  }

    /// <summary>
    /// Gets the entity type emitted by the DTO.
    /// </summary>
    EntityType Type { get; }

    string? GetContent();
}

/// <summary>
/// Extends <see cref="IRecord"/> with auditing information required for synchronization.
/// </summary>
public interface IVersionRecord
{
    /// <summary>
    /// Gets or sets the timestamp representing when the entity was created.
    /// </summary>
    DateTimeOffset CreatedAtUtc  { get; set; }

    /// <summary>
    /// Gets or sets the actor responsible for creation.
    /// </summary>
    string CreatedBy { get; set; }

    /// <summary>
    /// Gets or sets the timestamp of the most recent update.
    /// </summary>
    DateTimeOffset UpdatedAtUtc { get; set; }
    
    /// <summary>
    /// Gets or sets the monotonically increasing sequence used to order updates.
    /// </summary>
    long UpdatedSeq { get; set; }                

    /// <summary>
    /// Gets or sets the actor responsible for the most recent update.
    /// </summary>
    string? UpdatedBy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the entity has been soft-deleted.
    /// </summary>
    bool IsDeleted { get; set; } 

    /// <summary>
    /// Gets or sets the timestamp when the entity was soft-deleted, if applicable.
    /// </summary>
    DateTimeOffset? DeletedAt { get; set; }
}
