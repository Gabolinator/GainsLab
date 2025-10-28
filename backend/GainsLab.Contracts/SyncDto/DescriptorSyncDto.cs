using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncDto;

public record DescriptorSyncDto( 
    Guid GUID ,
    string DescriptionContent,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,              // monotonic tie-breaker
    bool IsDeleted = false) : ISyncDto
{
   
}
