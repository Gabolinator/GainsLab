using GainsLab.Application.DTOs;
using GainsLab.Application.DTOs.Description;
using GainsLab.Domain;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.Sync;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB;

public class DescriptorResolver : IDescriptorResolver
{
    public DescriptorResolver(string syncActor)
    {
        SyncActor = syncActor;
    }

    public string SyncActor { get; init; } 

    /// <summary>
    /// Resolves an existing descriptor or creates a placeholder when the remote payload omits one.
    /// </summary>
    public async Task<DescriptorRecord> ResolveDescriptorAsync(
        GainLabSQLDBContext dbContext,
        Guid? descriptorGuid,
        IDictionary<Guid, DescriptorRecord> cache,
        CancellationToken ct)
    {
        var key = descriptorGuid is null || descriptorGuid == Guid.Empty
            ? Guid.Empty
            : descriptorGuid.Value;

        if (cache.TryGetValue(key, out var cached))
            return cached;

        var descriptor = await dbContext.Descriptors
            .FirstOrDefaultAsync(d => d.GUID == key, ct)
            .ConfigureAwait(false);

        if (descriptor is null)
        {
            descriptor = new DescriptorRecord
            {
                GUID = key,
                Content = "none",
                CreatedAtUtc = DateTimeOffset.UtcNow,
                UpdatedAtUtc = DateTimeOffset.UtcNow,
                CreatedBy = SyncActor,
                UpdatedBy = SyncActor,
                Authority = DataAuthority.Bidirectional
            };

            await dbContext.Descriptors.AddAsync(descriptor, ct).ConfigureAwait(false);
        }

        cache[key] = descriptor;
        return descriptor;
    }
}