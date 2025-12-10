using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces.Entity;


namespace GainsLab.Domain.Entities.Descriptor;

/// <summary>
/// Container for descriptive metadata fields maintained separately from entity identity.
/// </summary>
public class BaseDescriptorContent
{
    
    
    /// <summary>
    /// Gets or sets the notes associated with the entity.
    /// </summary>
    public Notes? Notes { get; private set; } = new(null);
    
    /// <summary>
    /// Gets or sets the description of the entity.
    /// </summary>
    public Description? Description { get; set; }
    
    /// <summary>
    /// Gets or sets the Medias object of the entity.
    /// </summary>
    public MediaInfos? Medias { get; set; }

    /// <summary>
    /// Gets or sets the list of tags associated with the entity. This property is not mapped to the database.
    /// </summary>
    [NotMapped]
    public TagList? Tags { get; set; }
    
    /// <summary>
    /// Replaces the description with a new value.
    /// </summary>
    public void UpdateDescription(string? text) => Description = new Description(text);
    /// <summary>
    /// Replaces the notes with a new value.
    /// </summary>
    public void UpdateNotes(string? text) => Notes = new Notes(text);

    /// <summary>
    /// Performs validation of descriptor content and returns the current instance.
    /// </summary>
    public BaseDescriptorContent Validate()
    {
        //todo
        return this;
    }
}

/// <summary>
/// Aggregate root that couples descriptor identity, content, and audit metadata.
/// </summary>
public class BaseDescriptorEntity : EntityBase<DescriptorId, BaseDescriptorContent, AuditedInfo>, IEquatable<BaseDescriptorEntity>
{
    public BaseDescriptorEntity()
        : base(new DescriptorId(Guid.Empty), new BaseDescriptorContent(), new AuditedInfo(DateTimeOffset.UtcNow, "system"), -1)
    {
    }

    public BaseDescriptorEntity(DescriptorId id, BaseDescriptorContent content, AuditedInfo creationInfo, int dbId = -1)
        : base(id, content, creationInfo, dbId)
    {
    }

    public override EntityType Type => EntityType.Descriptor;
    public bool Equals(BaseDescriptorEntity? other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Id.Equals(other.Id);
    }

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BaseDescriptorEntity)obj);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Id, Content, CreationInfo);
    }
}
