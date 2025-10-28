using GainsLab.Core.Models.Core.Interfaces.DB;

namespace GainsLab.Contracts.SyncService;

/// <summary>
/// Stable cursor that captures the timestamp and sequence components used to resume sync pagination.
/// </summary>
/// <param name="Ts">The timestamp component of the cursor.</param>
/// <param name="Seq">The sequence component used to disambiguate identical timestamps.</param>
public sealed record SyncCursor(DateTimeOffset Ts, long Seq) : ISyncCursor
{
    /// <inheritdoc />
    public DateTimeOffset ITs => Ts;

    /// <inheritdoc />
    public long ISeq => Seq;
}

/// <summary>
/// Represents a server-issued page of synchronization DTOs and the cursor required to request the next page.
/// </summary>
/// <param name="ServerTime">The server timestamp associated with the payload.</param>
/// <param name="Next">The cursor to continue pagination, or <c>null</c> when no more data exists.</param>
/// <param name="Items">The materialized DTOs contained in this page.</param>
public sealed record SyncPage<TSyncDto>(
    DateTimeOffset ServerTime,
    SyncCursor? Next,
    IReadOnlyList<TSyncDto> Items
) : ISyncPage<ISyncDto> where TSyncDto : ISyncDto
{
    /// <inheritdoc />
    public IReadOnlyList<ISyncDto> ItemsList => Items as IReadOnlyList<ISyncDto> ?? Array.Empty<ISyncDto>();

    /// <inheritdoc />
    public ISyncCursor? NextPage => Next;

    /// <inheritdoc />
    public DateTimeOffset Time => ServerTime;
}
