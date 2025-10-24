using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces;
using GainsLab.Models.Core.Interfaces;

namespace GainsLab.Infrastructure.DB.Handlers;

public class BaseDescriptorService(IClock clock) : IDescriptorService<BaseDescriptorEntity>
{
    
  
    public BaseDescriptorEntity CreateFor(Guid guid)
    {
        //todo
        BaseDescriptorContent content = new BaseDescriptorContent();
        AuditedInfo creationInfo = AuditedInfo.New(clock.UtcNow,"test");
        return new BaseDescriptorEntity(new DescriptorId(guid), content, creationInfo);

    }

    public void Update(BaseDescriptorEntity descriptor)
    {
        //todo
    }
}