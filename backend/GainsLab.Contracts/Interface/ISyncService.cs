namespace GainsLab.Contracts.Interface;

public interface ISyncService
{
    Task PushAsync(CancellationToken ct = default);
    Task PullAsync(DateTimeOffset since, CancellationToken ct = default);
}