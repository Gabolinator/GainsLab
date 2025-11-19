using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.Sync;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Models.DataManagement;

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
    public async Task<DescriptorDTO> ResolveDescriptorAsync(
        GainLabSQLDBContext dbContext,
        Guid? descriptorGuid,
        IDictionary<Guid, DescriptorDTO> cache,
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
            descriptor = new DescriptorDTO
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