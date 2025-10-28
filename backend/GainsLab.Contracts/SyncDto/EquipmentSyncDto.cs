using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.SyncDto;

public record EquipmentSyncDto( 
 Guid GUID ,
 string Name,
 Guid? DescriptorGUID ,
DateTimeOffset UpdatedAtUtc,
long UpdatedSeq,              // monotonic tie-breaker
bool IsDeleted = false) : ISyncDto
{
    
    
   
}