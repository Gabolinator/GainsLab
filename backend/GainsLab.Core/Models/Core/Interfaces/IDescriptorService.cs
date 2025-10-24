namespace GainsLab.Core.Models.Core.Interfaces;

/// <summary>
/// Provides CRUD helpers for descriptor entities tied to aggregates.
/// </summary>
public interface IDescriptorService<TDescriptor>
{
    TDescriptor CreateFor(Guid guid);
    void Update(TDescriptor descriptor);
}
