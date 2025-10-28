using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncService;

public sealed record SyncCursor(DateTimeOffset Ts, long Seq) : ISyncCursor
{
    public DateTimeOffset ITs => Ts;
    public long ISeq => Seq;
}; // stable cursor


public sealed record SyncPage<TSyncDto>(
    DateTimeOffset ServerTime,
    SyncCursor? Next, // null when no more pages
    IReadOnlyList<TSyncDto> Items
) :  ISyncPage<ISyncDto> where TSyncDto : ISyncDto
{
    public IReadOnlyList<ISyncDto> ItemsList => Items as IReadOnlyList<ISyncDto> ?? Array.Empty<ISyncDto>();
    public ISyncCursor? NextPage => Next;
    public DateTimeOffset Time => ServerTime;
}


