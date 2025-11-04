namespace GainsLab.Core.Models.Core.Interfaces.DB;

public interface IDto
{
    public int Iid { get; }
    public Guid  Iguid { get;  }

    public EntityType Type { get; }

}

public interface IVersionedDto
{
    public DateTimeOffset CreatedAtUtc  { get; set; }
    public string CreatedBy { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    
    public long UpdatedSeq { get; set; }                
    public string? UpdatedBy { get; set; }
    public bool IsDeleted { get; set; } 
    public DateTimeOffset? DeletedAt { get; set; }
}