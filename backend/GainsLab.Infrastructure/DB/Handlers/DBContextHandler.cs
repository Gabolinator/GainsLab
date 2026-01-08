using System.Linq;
using System.Reflection;
using GainsLab.Application;
using GainsLab.Application.Results;
using GainsLab.Contracts.Interface;
using GainsLab.Domain.Interfaces;
using GainsLab.Domain.Interfaces.Entity;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Base EF Core handler that implements common add/update logic for Record repositories.
/// </summary>
public abstract class IdbContextHandler<TRecord> : IDBHandler where TRecord : class, IRecord
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdbContextHandler{TRecord}"/> class.
    /// </summary>
    /// <param name="context">EF Core context used to interact with the underlying database.</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    protected IdbContextHandler(DbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> used to query and persist Records.
    /// </summary>
    public abstract DbSet<TRecord> DBSet { get; }
    
    protected DbContext _context;
    protected readonly ILogger _logger;
    
    /// <summary>
    /// Gives derived handlers a chance to attach/ensure related entities before persisting.
    /// </summary>
    protected virtual Task PrepareRelatedEntitiesAsync(TRecord Record, CancellationToken ct) =>
        Task.CompletedTask;
    private static readonly string GuidPropertyName = ResolveGuidPropertyName();
    private static readonly string IdPropertyName = ResolveIdPropertyName();
    private static readonly PropertyInfo GuidPropertyInfo = typeof(TRecord).GetProperty(GuidPropertyName)
        ?? throw new InvalidOperationException($"Type {typeof(TRecord).Name} must expose a public GUID property.");
    private static readonly PropertyInfo IdPropertyInfo = typeof(TRecord).GetProperty(IdPropertyName)
        ?? throw new InvalidOperationException($"Type {typeof(TRecord).Name} must expose a public int ID property.");
    
    /// <summary>
    /// Attempts to load an existing Record by GUID.
    /// </summary>
    public abstract Task<Result<TRecord>> TryGetExistingRecord(Guid guid, string? content);
   
    /// <summary>
    /// Attempts to load an existing Record by integer identifier.
    /// </summary>
    public async Task<Result<TRecord>> TryGetExistingRecord(int id, string? content)
    {
        if (id <= 0)
            return Result<TRecord>.Failure("Invalid Record id");

        var existing = await FilterById(DBSet.AsNoTracking(), id)
            .FirstOrDefaultAsync(CancellationToken.None);
        var success = existing is not null;
        return success
            ? Result<TRecord>.SuccessResult(existing!)
            : Result<TRecord>.Failure("No existing Record found");
    }


    /// <inheritdoc />
    public async Task<Result<IRecord>> AddAsync(IRecord Record, bool save, CancellationToken ct = default)
    {
        if (Record is not TRecord tRecord)
            return Result<IRecord>.Failure("Invalid Record type");

        try
        {
            await PrepareRelatedEntitiesAsync(tRecord, ct).ConfigureAwait(false);
            // Optionally stamp server fields here if needed
            DBSet.Add(tRecord); // tracked as Added

            if (save)
                await _context.SaveChangesAsync(ct);

            return Result<IRecord>.SuccessResult(tRecord);
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Add failed for {Record.Iguid}: {ex.GetBaseException().Message}");
            return Result<IRecord>.Failure(ex.GetBaseException().Message);
        }
    }

    /// <inheritdoc />
    public async Task<Result<IRecord>> UpdateAsync(IRecord Record, bool save, CancellationToken ct = default)
    {
        if (Record is not TRecord tRecord)
            return Result<IRecord>.Failure("Invalid Record type");

        try
        {
            await PrepareRelatedEntitiesAsync(tRecord, ct).ConfigureAwait(false);
            // Ensure we’re not double-tracking the same key
            var existing = await LocateExistingAsync(tRecord, ct);
            if (existing is null)
                return Result<IRecord>.Failure("Record to update not found");

            EnsurePersistentKeyValues(tRecord, existing);

            // Attach and mark modified (full replace pattern)
            _context.Attach(tRecord);
            _context.Entry(tRecord).State = EntityState.Modified;

            // If you want partial updates instead (safer), copy fields:
            // _context.Attach(localEntity);
            // localEntity.Name = tRecord.Name; ... then Save (state stays Unchanged, EF detects changed members)

            if (save)
                await _context.SaveChangesAsync(ct);

            return Result<IRecord>.SuccessResult(tRecord);
        }
        catch (DbUpdateConcurrencyException cex)
        {
            _logger.LogError("DbContextHandler", $"Concurrency on update {Record.Iguid}: {cex.GetBaseException().Message}");
            return Result<IRecord>.Failure("Concurrency conflict");
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Update failed for {Record.Iguid}: {ex.GetBaseException().Message}");
            return Result<IRecord>.Failure(ex.GetBaseException().Message);
        }
    }

 


    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<IRecord>>> AddOrUpdateAsync(
        IReadOnlyList<IRecord> Records, bool save = true, CancellationToken ct = default)
    {
        if (Records is null || Records.Count == 0)
            return Result<IReadOnlyList<IRecord>>.Failure("No Records");

        var saved = new List<IRecord>(Records.Count);

        // Only create a transaction when we intend to save
        await using var tx = save ? await _context.Database.BeginTransactionAsync(ct) : null;

        // Speed up large batches
        var originalAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            foreach (var Record in Records)
            {
                ct.ThrowIfCancellationRequested();

                var r = await AddOrUpdateAsync(Record, save: false, ct);
                if (!r.Success || r.Value is null)
                {
                    // Fail fast -> rollback whole batch for atomicity
                    var reason = r.GetErrorMessage() ?? "Unknown error";
                    return Result<IReadOnlyList<IRecord>>.Failure($"Failed on Record {Record.GetType().Name}: {reason}");
                }

                saved.Add(r.Value);
            }

            if (save)
            {
                // run a final detect & persist once
                _context.ChangeTracker.DetectChanges();
                await _context.SaveChangesAsync(ct);
                await tx!.CommitAsync(ct);
            }

            return Result<IReadOnlyList<IRecord>>.SuccessResult(saved);
        }
        catch (OperationCanceledException)
        {
            if (save && tx is not null) await tx.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            if (save && tx is not null) await tx.RollbackAsync(CancellationToken.None);
            return Result<IReadOnlyList<IRecord>>.Failure($"Batch add/update failed: {ex.GetBaseException().Message}");
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
        }
    }

    /// <summary>
    /// Filters the given queryable to match the provided GUID using the mapped store column.
    /// </summary>
    protected virtual IQueryable<TRecord> FilterByGuid(IQueryable<TRecord> query, Guid guid) =>
        query.Where(Record => EF.Property<Guid>(Record, GuidPropertyName) == guid);

    /// <summary>
    /// Filters the given queryable to match the provided integer identifier using the mapped store column.
    /// </summary>
    protected virtual IQueryable<TRecord> FilterById(IQueryable<TRecord> query, int id) =>
        query.Where(Record => EF.Property<int>(Record, IdPropertyName) == id);
    
    private static string ResolveGuidPropertyName()
    {
        var guidProp = typeof(TRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.PropertyType == typeof(Guid) &&
                                 (string.Equals(p.Name, "GUID", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(p.Name, "Guid", StringComparison.OrdinalIgnoreCase)));

        if (guidProp is null)
            throw new InvalidOperationException(
                $"Type {typeof(TRecord).Name} must expose a public GUID property for filtering.");

        return guidProp.Name;
    }

    private static string ResolveIdPropertyName()
    {
        var idProp = typeof(TRecord).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.PropertyType == typeof(int) &&
                                 (string.Equals(p.Name, "ID", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)));

        if (idProp is null)
            throw new InvalidOperationException(
                $"Type {typeof(TRecord).Name} must expose a public int ID property for filtering.");

        return idProp.Name;
    }

    private async Task<TRecord?> LocateExistingAsync(TRecord Record, CancellationToken ct)
    {
        var query = DBSet.AsNoTracking();

        if (Record.Iid > 0)
        {
            var byId = await FilterById(query, Record.Iid).FirstOrDefaultAsync(ct);
            if (byId is not null)
                return byId;
        }

        if (Record.Iguid != Guid.Empty)
        {
            var byGuid = await FilterByGuid(query, Record.Iguid).FirstOrDefaultAsync(ct);
            if (byGuid is not null)
                return byGuid;
        }

        var content = Record.GetContent();
        if (!string.IsNullOrWhiteSpace(content))
        {
            var contentResult = await TryGetExistingRecord(Guid.Empty, content);
            if (contentResult.Success && contentResult.Value is not null)
                return contentResult.Value;
        }

        return null;
    }

    private static void EnsurePersistentKeyValues(TRecord target, TRecord existing)
    {
        if (target is null || existing is null) return;

        var existingId = (int)(IdPropertyInfo.GetValue(existing) ?? 0);
        if (existingId > 0)
            IdPropertyInfo.SetValue(target, existingId);

        var existingGuid = (Guid?)(GuidPropertyInfo.GetValue(existing));
        if (existingGuid is not null && existingGuid != Guid.Empty)
            GuidPropertyInfo.SetValue(target, existingGuid);
    }


    /// <inheritdoc />
    public async Task<Result<IRecord>> AddOrUpdateAsync(IRecord Record, bool save, CancellationToken ct = default)
    {
        _logger.Log("DbContextHandler", $"Trying to add or update Record {Record.Iguid}");

        if (Record is not TRecord tRecord)
        {
            _logger.LogWarning("DbContextHandler", $"Cant add or update Record {Record.Iguid} - wrong type");
            return Result<IRecord>.Failure("Invalid Record type");
        }

        var existing = await LocateExistingAsync(tRecord, ct);
        if (existing is not null)
        {
            EnsurePersistentKeyValues(tRecord, existing);

            if (!NeedUpdate(existing, tRecord))
            {
                _logger.Log("DbContextHandler", $"No update needed for {Record.Iguid}");
                return Result<IRecord>.SuccessResult(existing);
            }

            return await UpdateAsync(tRecord, save, ct);
        }

        // Not found, add
        return await AddAsync(tRecord, save, ct);
    }

    /// <summary>
    /// Return true if 'incoming' should overwrite 'existing'.
    /// Prefer server-authoritative stamps like UpdatedAtUtc/UpdatedSeq.
    /// </summary>
    private bool NeedUpdate(TRecord existingRecord, TRecord incomingRecord)
    {
        // if versioned Records
        if (existingRecord is IVersionRecord ex && incomingRecord is IVersionRecord inc)
        {
            // incoming is newer if timestamp is greater or same ts with higher seq
            return inc.UpdatedAtUtc > ex.UpdatedAtUtc
                   || (inc.UpdatedAtUtc == ex.UpdatedAtUtc && inc.UpdatedSeq > ex.UpdatedSeq);
        }

        //deep equality means no update
        if (existingRecord.Equals(incomingRecord)) return false;

        // Fallback: consider any difference as needing update
        return true;
    }
    // public async Task<Result<IRecord>> AddAsync(IRecord Record, bool save,CancellationToken ct = default)
    // {
    //     if (Record is not TRecord tRecord) return Result<IRecord>.Failure("Invalid Record type");
    //     return await AddAsync(tRecord, save);
    // }

    // public async Task<Result<IRecord>> UpdateAsync(IRecord Record, bool save, CancellationToken ct = default)
    // {
    //     if (Record is not TRecord tRecord) return Result<IRecord>.Failure("Invalid Record type");;
    //    return await UpdateAsync(tRecord, save, ct);
    // }

    public abstract Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default);

    public string NormalizeContent(string value, Func<string, string> normalizeStrategy) =>
        normalizeStrategy.Invoke(value);

}
