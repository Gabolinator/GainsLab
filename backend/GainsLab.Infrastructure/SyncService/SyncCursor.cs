using System.Globalization;
using GainsLab.Contracts.Interface;

namespace GainsLab.Infrastructure.SyncService;

public static class SyncCursorUtil                                                                                                                                     
{                                                                                                                                                                  
    public static ISyncCursor MinValue { get; } = new InMemorySyncCursor(DateTimeOffset.MinValue, 0);                                                              
                                                                                                                                                                     
    private sealed record InMemorySyncCursor(DateTimeOffset ITs, long ISeq) : ISyncCursor;

    /// <summary>
    /// Serializes a cursor into a URL-safe string token.
    /// Example: "2025-10-28T14:33:10.1234567Z|42"
    /// </summary>
    public static string ToToken(ISyncCursor cursor)
    {
        if (cursor == null)
            throw new ArgumentNullException(nameof(cursor));

        // ISO8601 timestamp with UTC normalization + sequence separated by '|'
        // Using "O" (round-trip) format guarantees full precision and correct round-tripping.
        return $"{cursor.ITs.UtcDateTime:O}|{cursor.ISeq}";
    }

    /// <summary>
    /// Parses a token produced by <see cref="ToToken"/> back into an <see cref="ISyncCursor"/>.
    /// Returns null if the format is invalid.
    /// </summary>
    public static ISyncCursor? Parse(string token)
    {
        if (string.IsNullOrWhiteSpace(token))
            return null;

        var parts = token.Split('|', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length != 2)
            return null;

        if (!DateTimeOffset.TryParse(parts[0], CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var ts))
            return null;

        if (!long.TryParse(parts[1], NumberStyles.Integer, CultureInfo.InvariantCulture, out var seq))
            return null;

        return new InMemorySyncCursor(ts, seq);
    }
}                                                                 