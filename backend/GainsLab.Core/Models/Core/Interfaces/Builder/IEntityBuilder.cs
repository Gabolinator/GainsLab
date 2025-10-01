namespace GainsLab.Core.Models.Core.Interfaces.Builder;

public interface IEntityBuilder<TEntity, TId, TContent, TAudit, TDescriptor>
{
    IEntityBuilder<TEntity, TId, TContent, TAudit, TDescriptor> WithId(TId id);
    IEntityBuilder<TEntity, TId, TContent, TAudit, TDescriptor> WithContent(TContent content);
    IEntityBuilder<TEntity, TId, TContent, TAudit, TDescriptor> WithAudit(TAudit audit);
    IEntityBuilder<TEntity, TId, TContent, TAudit, TDescriptor> WithDescriptor(TDescriptor descriptor);
    TEntity Build();
}