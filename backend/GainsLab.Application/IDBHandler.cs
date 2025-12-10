using GainsLab.Application.Results;
using GainsLab.Contracts.Interface;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Application;

/// <summary>
/// Defines persistence operations for DTOs managed by the local repository.
/// </summary>
public interface IDBHandler
{
    /// <summary>
    /// Adds or updates the supplied DTO collection in a single batch.
    /// </summary>
    /// <param name="dto">DTOs to persist.</param>
    /// <param name="save"><c>true</c> to flush changes immediately.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<Result<IReadOnlyList<IRecord>>> AddOrUpdateAsync(IReadOnlyList<IRecord> dto, bool save ,CancellationToken ct);

    /// <summary>
    /// Adds or updates a single DTO instance.
    /// </summary>
    /// <param name="record">DTO to persist.</param>
    /// <param name="save"><c>true</c> to flush changes immediately.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<Result<IRecord>> AddOrUpdateAsync(IRecord record, bool save,CancellationToken ct = default);

    /// <summary>
    /// Adds a new DTO to the underlying store.
    /// </summary>
    /// <param name="record">DTO to insert.</param>
    /// <param name="save"><c>true</c> to flush changes immediately.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<Result<IRecord>> AddAsync(IRecord record, bool save,CancellationToken ct = default);

    /// <summary>
    /// Updates an existing DTO in the underlying store.
    /// </summary>
    /// <param name="record">DTO to update.</param>
    /// <param name="save"><c>true</c> to flush changes immediately.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<Result<IRecord>> UpdateAsync(IRecord record, bool save,CancellationToken ct = default);

    /// <summary>
    /// Retrieves all entities mapped from DTOs handled by this repository.
    /// </summary>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default);
}
