using System.ComponentModel.DataAnnotations;
using GainsLab.Core.Models.Core.Interfaces.DB;


namespace GainsLab.Infrastructure.DB.DTOs;

public abstract class BaseDto : IDto
{
    
    public abstract int Iid { get;} 
    public abstract Guid  Iguid { get; }
    
    public DateTimeOffset CreatedAtUtc  { get; set; } = DateTimeOffset.UtcNow;
    public string CreatedBy { get; set; } = "system";
    public DateTimeOffset UpdatedAtUtc { get; set; } = DateTimeOffset.UtcNow;
    
    public long UpdatedSeq { get; set; }                 // monotonic tie-breaker
    public string? UpdatedBy { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; } = null;
    public string? DeletedBy { get; set; } = null;
    
    [Timestamp] // works with SQLite rowversion; for Postgres we map separately (see below)
    public byte[]? RowVersion { get; set; }
    
    public long Version { get; set; } = 0;
}