using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Contracts.Outbox;

public interface IOutboxDispatcher
{
    Task<Result> DispatchAsync(CancellationToken ct);            
}