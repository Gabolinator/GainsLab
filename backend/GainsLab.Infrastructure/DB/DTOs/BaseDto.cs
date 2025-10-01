using System.ComponentModel.DataAnnotations;
using GainsLab.Models.DataManagement.DB.Model.DTOs;

namespace GainsLab.Infrastructure.DB.DTOs;

public abstract class BaseDto : IDto
{
    
    public abstract int Iid { get;} 
    public abstract Guid  Iguid { get; }
    
    public DateTimeOffset CreatedAtUtc  { get; set; }
    public string CreatedBy  { get; set; }
    public DateTimeOffset? UpdatedAtUtc { get; set; } = null;
    public string? UpdatedBy { get; set; } = null;
    public bool IsDeleted { get; set; } = false;
    public DateTimeOffset? DeletedAt { get; set; } = null;
    public string? DeletedBy { get; set; } = null;
    
    [Timestamp] // works with SQLite rowversion; for Postgres we map separately (see below)
    public byte[]? RowVersion { get; set; }
    
    public long Version { get; set; } = 0;
}