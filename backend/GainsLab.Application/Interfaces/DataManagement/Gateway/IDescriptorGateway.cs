using GainsLab.Application.DTOs.Description;
using GainsLab.Application.Results;

namespace GainsLab.Application.Interfaces.DataManagement.Gateway;

public interface IDescriptorGateway
{
    public bool TryGetDescriptor(Guid descriptorGuid, out DescriptorRecord? descriptor);
    public Task<Result<DescriptorRecord>> TryGetDescriptorAsync(Guid? descriptorGuid);
    public Task<Result<IReadOnlyList<DescriptorRecord>>> GetAllDescriptorAsync();
    Task UpdateCacheAsync();
}