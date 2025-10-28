namespace GainsLab.Core.Models.Core.Interfaces.DB;

/// <summary>
/// Represents a single page of synchronization payload obtained from a remote source.
/// </summary>
public interface ISyncPage<TSyncDto> where TSyncDto : ISyncDto
{
    /// <summary>
    /// Gets the items contained in this page, strongly typed to the DTO.
    /// </summary>
    IReadOnlyList<TSyncDto> ItemsList { get; }

    /// <summary>
    /// Gets the cursor to request the subsequent page, or <c>null</c> when no further data is available.
    /// </summary>
    ISyncCursor? NextPage { get; }

    /// <summary>
    /// Gets the server timestamp associated with this page of results.
    /// </summary>
    DateTimeOffset Time { get; }
}

/// <summary>
/// Marker type for data transfer objects participating in the synchronization pipeline.
/// </summary>
public interface ISyncDto
{
}

/// <summary>
/// Cursor abstraction that captures the remote position within a sync stream.
/// </summary>
public interface ISyncCursor
{
    /// <summary>
    /// Gets the timestamp component for the cursor.
    /// </summary>
    DateTimeOffset ITs { get; }

    /// <summary>
    /// Gets the sequence number used as a tiebreaker for stable ordering.
    /// </summary>
    long ISeq { get; }
}
