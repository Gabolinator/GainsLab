using GainsLab.Application.DTOs;
using GainsLab.Infrastructure.DB.Context;

namespace GainsLab.Infrastructure.Sync;

public interface IDescriptorResolver
{
    public string SyncActor { get; init; }

    public Task<DescriptorRecord> ResolveDescriptorAsync(
        GainLabSQLDBContext dbContext,
        Guid? descriptorGuid,
        IDictionary<Guid, DescriptorRecord> cache,
        CancellationToken ct);
    
}