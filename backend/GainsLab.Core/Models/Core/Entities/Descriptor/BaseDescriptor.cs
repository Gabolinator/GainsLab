
using System.ComponentModel.DataAnnotations.Schema;
using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;


namespace GainsLab.Core.Models.Core.Entities.Descriptor;

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
    
    
    public void UpdateDescription(string? text) => Description = new Description(text);
    public void UpdateNotes(string? text) => Notes = new Notes(text);

    public BaseDescriptorContent Validate()
    {
        //todo
        return this;
    }
}



public class BaseDescriptorEntity : EntityBase<DescriptorId,BaseDescriptorContent, AuditedInfo> , IEquatable<BaseDescriptorEntity>
{
    public BaseDescriptorEntity()
    {
        
    }

    public BaseDescriptorEntity(DescriptorId id, BaseDescriptorContent content, AuditedInfo creationInfo)
    {
        Id = id;
        Content = content;
        CreationInfo = creationInfo;
    }

    public DescriptorId Id { get; }
    public BaseDescriptorContent Content { get; }
    public AuditedInfo CreationInfo { get; }

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