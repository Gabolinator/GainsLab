using GainsLab.Contracts.Interface;
using GainsLab.Contracts.SyncDto;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Infrastructure.DB.Context;
using GainsLab.Infrastructure.DB.DTOs;

namespace GainsLab.Contracts.SyncService;

public class MovementCategorySyncService : ISyncService<MovementCategorySyncDto>
{
    private readonly GainLabPgDBContext _db;
    private readonly Core.Models.Core.Utilities.Logging.ILogger _log;
    private const string SyncActor = "sync";
    private static readonly Guid PlaceholderDescriptorGuid = Guid.Empty;
    private readonly Dictionary<Guid, DescriptorDTO> _descriptorCache = new();
    
    public MovementCategorySyncService(GainLabPgDBContext db, Core.Models.Core.Utilities.Logging.ILogger log)
    {
        _db = db;
        _log = log;
    }

    /// <summary>
    /// Gets the entity type handled by this service.
    /// </summary>
    public EntityType EntityType => EntityType.MovementCategory;
    /// <summary>
    /// Gets the DTO type used for serialization.
    /// </summary>
    public Type DtoType => typeof(MovementCategorySyncDto);
    
    Task<PushResult> ISyncService.PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct)
        => PushAsync(dtos.Cast<MovementCategorySyncDto>(), ct);

    async Task<object> ISyncService.PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct)
        => await PullAsync(cur, take, ct);


    public Task<SyncPage<MovementCategorySyncDto>> PullAsync(SyncCursor cur, int take, CancellationToken ct)
    {
        
    }

    public Task<PushResult> PushAsync(IEnumerable<MovementCategorySyncDto> items, CancellationToken ct)
    {
      
    }
}