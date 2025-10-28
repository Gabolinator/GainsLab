using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Infrastructure.DB.Outbox;

public interface IOutboxDispatcher
{
    Task<Result> DispatchAsync(CancellationToken ct);            
}