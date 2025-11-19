using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Models.DataManagement.Sync;

public interface IDescriptorResolver
{
    public string SyncActor { get; init; }

    public Task<DescriptorDTO> ResolveDescriptorAsync(
        GainLabSQLDBContext dbContext,
        Guid? descriptorGuid,
        IDictionary<Guid, DescriptorDTO> cache,
        CancellationToken ct);
    
}