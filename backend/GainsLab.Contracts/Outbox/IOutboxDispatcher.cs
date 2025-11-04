using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Contracts.Outbox;

/// <summary>
/// Dispatches pending outbox entries to the upstream sync API.
/// </summary>
public interface IOutboxDispatcher
{
    /// <summary>
    /// Sends unsent outbox entries to the server and marks successful rows accordingly.
    /// </summary>
    /// <param name="ct">Cancellation token propagated from the caller.</param>
    Task<Result> DispatchAsync(CancellationToken ct);            
}
