using GainsLab.Contracts.SyncService;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.Interface;

/// <summary>
/// Defines the contract for a synchronization service that can page data for a specific entity type.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Gets the entity type handled by this service.
    /// </summary>
    EntityType EntityType { get; }

    /// <summary>
    /// Gets the concrete DTO type handled by this service.
    /// </summary>
    Type DtoType { get; }
    
    /// <summary>
    /// Pushes local changes upstream using the configured transport, if supported.
    /// </summary>
    /// <param name="dtos">The DTO instances to push.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<PushResult> PushBoxedAsync(IEnumerable<ISyncDto> dtos, CancellationToken ct);

    /// <summary>
    /// Retrieves a boxed page of DTOs for the associated entity type.
    /// </summary>
    /// <param name="cur">Cursor describing the starting point.</param>
    /// <param name="take">Maximum number of items to include.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A task that resolves to a boxed page of synchronization DTOs.</returns>
    Task<object> PullBoxedAsync(SyncCursor cur, int take, CancellationToken ct);
}

/// <summary>
/// Strongly-typed variant of <see cref="ISyncService"/> that returns DTOs of type <typeparamref name="T"/>.
/// </summary>
public interface ISyncService<T> : ISyncService where T : ISyncDto
{
    /// <summary>
    /// Retrieves a typed page of synchronization DTOs for the associated entity type.
    /// </summary>
    /// <param name="cur">Cursor describing the starting point.</param>
    /// <param name="take">Maximum number of items to include.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<SyncPage<T>> PullAsync(SyncCursor cur, int take, CancellationToken ct);
    
    /// <summary>
    /// Pushes typed DTOs upstream using the configured transport.
    /// </summary>
    /// <param name="items">The typed DTO instances to push.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<PushResult> PushAsync(IEnumerable<T> items, CancellationToken ct);
}
