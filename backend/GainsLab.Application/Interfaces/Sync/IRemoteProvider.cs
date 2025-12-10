using GainsLab.Application.Results;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;

namespace GainsLab.Application.Interfaces.Sync;

/// <summary>
/// Abstracts access to the upstream synchronization service (typically an HTTP API).
/// </summary>
public interface IRemoteProvider
{
    /// <summary>
    /// Performs any one-time initialization such as authentication or connectivity checks.
    /// </summary>
    Task<Result> InitializeAsync();

    /// <summary>
    /// Pulls a page of remote changes for the specified entity type using the supplied cursor.
    /// </summary>
    /// <param name="type">Entity type being synchronized.</param>
    /// <param name="cursor">Cursor indicating where to resume the remote stream.</param>
    /// <param name="take">Maximum number of results to request from the server.</param>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    /// <returns>A result containing the remote page payload or an error.</returns>
    Task<Result<ISyncPage<ISyncDto>>> PullAsync(EntityType type, ISyncCursor cursor, int take = 200,
        CancellationToken ct = default);
}
