namespace GainsLab.Domain.Interfaces.Entity;


//todo 
//use this architecture moving forward
//and refactor all



/// <summary>
/// Marker for domain entities exposed by the GainsLab core model.
/// </summary>
public interface IEntity
{
    public EntityType Type { get;}

}


//container object to map its content with its id (ID for db, with string id)
/// <summary>
/// Describes an entity with identity and content payload.
/// </summary>
public interface IEntity<TId, TContent> : IEntity
{
    
    TId Id { get; }
    TContent Content { get; }
}

//for entities that carry audit/creation info
/// <summary>
/// Indicates that an entity exposes audit metadata.
/// </summary>
public interface IAudited<TCreationInfo>
{
    TCreationInfo CreationInfo { get; }
}

/// <summary>
/// Indicates that an entity exposes descriptor metadata.
/// </summary>
public interface IDescribed<TDescriptor>
{
    TDescriptor Descriptor { get; }
}

/// <summary>
/// Combines identity, content, and audit information for an entity.
/// </summary>
public interface IEntity<TId,TContent, TCreationInfo> : IEntity<TId,TContent> , IAudited<TCreationInfo>
{
    
}


/// <summary>
/// Base implementation providing common identity, content, and audit handling.
/// </summary>
public abstract class EntityBase<TId,TContent, TCreationInfo> : IEntity<TId,TContent, TCreationInfo>
{
    // DB primary key (clustered), assigned on insert
    public int DbId { get; protected set; } 
    public TId Id { get; init; }
    public TContent Content { get; protected init; }
    public TCreationInfo CreationInfo { get; protected init; }

    public abstract EntityType Type { get; }

    protected EntityBase() { } 
    protected EntityBase(TId id, TContent content, TCreationInfo creation, int dbId)
    {
         
        Content = content ?? throw new ArgumentNullException(nameof(content));
        Id = id ?? throw new ArgumentNullException(nameof(id));
        CreationInfo = creation ?? throw new ArgumentNullException(nameof(creation));
        DbId = dbId;

    }
}










  
