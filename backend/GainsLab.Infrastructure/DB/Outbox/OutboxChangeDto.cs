using System.ComponentModel.DataAnnotations;

namespace GainsLab.Infrastructure.DB.Outbox;


public enum ChangeType { Insert, Update, Delete }


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