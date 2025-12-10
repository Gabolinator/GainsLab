using System.Text.Json;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.SyncService;

namespace GainsLab.Infrastructure.Sync;

/// <summary>
/// Persists sync cursors to disk so that desktop clients can resume incremental pulls.
/// </summary>
public sealed class FileSyncCursorStore : ISyncCursorStore
{
    private readonly ILogger _logger;
    private readonly string _rootPath;
    private readonly JsonSerializerOptions _serializerOptions = new(JsonSerializerDefaults.Web);
    private readonly SemaphoreSlim _mutex = new(1, 1);

    /// <summary>
    /// Creates a new file-backed cursor store rooted under the user's local-app-data.
    /// </summary>
    public FileSyncCursorStore(ILogger logger)
    {
        _logger = logger;
        _rootPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "GainsLab",
            "SyncCursors");

        Directory.CreateDirectory(_rootPath);
    }

    /// <inheritdoc />
    public async Task<ISyncCursor?> GetCursorAsync(EntityType type, CancellationToken ct)
    {
        var path = GetPath(type);
        if (!File.Exists(path))
            return null;

        await _mutex.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await using var stream = File.OpenRead(path);
            var payload = await JsonSerializer.DeserializeAsync<CursorPayload>(stream, _serializerOptions, ct)
                          .ConfigureAwait(false);
            return payload is null ? null : new SyncCursor(payload.Ts, payload.Seq);
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(FileSyncCursorStore),
                $"Failed to load cursor for {type}: {ex.Message}");
            return null;
        }
        finally
        {
            _mutex.Release();
        }
    }

    /// <inheritdoc />
    public async Task SaveCursorAsync(EntityType type, ISyncCursor cursor, CancellationToken ct)
    {
        var path = GetPath(type);
        var payload = new CursorPayload(cursor.ITs, cursor.ISeq);

        await _mutex.WaitAsync(ct).ConfigureAwait(false);
        try
        {
            await using var stream = File.Create(path);
            await JsonSerializer.SerializeAsync(stream, payload, _serializerOptions, ct).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(FileSyncCursorStore),
                $"Failed to persist cursor for {type}: {ex.Message}");
        }
        finally
        {
            _mutex.Release();
        }
    }

    private string GetPath(EntityType type) => Path.Combine(_rootPath, $"{type}.json");

    private sealed record CursorPayload(DateTimeOffset Ts, long Seq);
}
