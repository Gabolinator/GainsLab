using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Models.Core;
using GainsLab.Models.Core.Descriptor;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Core.Models.Core.Interfaces.Entity;


//todo 
//use this architecture moving forward
//and refactor all



public interface IEntity
{
    public EntityType Type { get;}

}


//container object to map its content with its id (ID for db, with string id)
public interface IEntity<TId, TContent> : IEntity
{
    
    TId Id { get; }
    TContent Content { get; }
}

//for entities that carry audit/creation info
public interface IAudited<TCreationInfo>
{
    TCreationInfo CreationInfo { get; }
}

public interface IDescribed<TDescriptor>
{
    TDescriptor Descriptor { get; }
}

public interface IEntity<TId,TContent, TCreationInfo> : IEntity<TId,TContent> , IAudited<TCreationInfo>
{
    
}


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










  