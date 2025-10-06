namespace GainsLab.Core.Models.Core.Interfaces;

public interface IDescriptorService<TDescriptor>
{
    TDescriptor CreateFor(Guid guid);
    void Update(TDescriptor descriptor);
}