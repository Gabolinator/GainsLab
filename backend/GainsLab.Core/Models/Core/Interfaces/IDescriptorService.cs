using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;

namespace GainsLab.Core.Models;

public interface IDescriptorService<TDescriptor>
{
    TDescriptor CreateFor(Guid guid);
    void Update(TDescriptor descriptor);
}