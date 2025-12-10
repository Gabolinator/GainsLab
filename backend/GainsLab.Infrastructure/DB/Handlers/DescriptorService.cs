using GainsLab.Application.Interfaces;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces;


namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Minimal descriptor service used during seeding and domain construction.
/// </summary>
public class BaseDescriptorService(IClock clock) : IDescriptorService<BaseDescriptorEntity>
{
    
  
    /// <inheritdoc />
    public BaseDescriptorEntity CreateFor(Guid guid)
    {
        //todo
        BaseDescriptorContent content = new BaseDescriptorContent();
        AuditedInfo creationInfo = AuditedInfo.New(clock.UtcNow,"test");
        return new BaseDescriptorEntity(new DescriptorId(guid), content, creationInfo);

    }

    /// <inheritdoc />
    public void Update(BaseDescriptorEntity descriptor)
    {
        //todo
    }
}
