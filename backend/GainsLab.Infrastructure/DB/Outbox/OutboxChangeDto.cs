using System.ComponentModel.DataAnnotations;

namespace GainsLab.Infrastructure.DB.Outbox;


/// <summary>
/// Enumerates the mutation types captured in the outbox table.
/// </summary>
public enum ChangeType { Insert, Update, Delete }


/// <summary>
/// Represents a single outbox record queued for upstream dispatch.
/// </summary>
public class OutboxChangeDto
{
    [Key]
    public long Id { get; set; } // identity
    public string Entity { get; set; } = default!;  // e.g., "User" based on enum EntityType
    public Guid EntityGuid { get; set; }
    public ChangeType ChangeType { get; set; }
    public string PayloadJson { get; set; } = default!; // full entity snapshot or minimal delta
    public DateTimeOffset OccurredAt { get; set; } = DateTimeOffset.UtcNow;
    public bool Sent { get; set; }
    
    public override string ToString()
    {
        return $"{nameof(OutboxChangeDto)} - " +
               $"ID : {Id} | " +
               $"Entity Type {Entity} |" +
               $"EntityGuid :{EntityGuid} |"+
               $"ChangeType :{ChangeType} ";
    }
    
}
