using System.Linq;
using System.Reflection;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

/// <summary>
/// Base EF Core handler that implements common add/update logic for DTO repositories.
/// </summary>
public abstract class IdbContextHandler<TDto> : IDBHandler where TDto : class, IDto
{
    /// <summary>
    /// Initializes a new instance of the <see cref="IdbContextHandler{TDto}"/> class.
    /// </summary>
    /// <param name="context">EF Core context used to interact with the underlying database.</param>
    /// <param name="logger">Logger used for diagnostic output.</param>
    protected IdbContextHandler(DbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Gets the <see cref="DbSet{TEntity}"/> used to query and persist DTOs.
    /// </summary>
    public abstract DbSet<TDto> DBSet { get; }
    
    protected DbContext _context;
    protected readonly ILogger _logger;
    
    /// <summary>
    /// Gives derived handlers a chance to attach/ensure related entities before persisting.
    /// </summary>
    protected virtual Task PrepareRelatedEntitiesAsync(TDto dto, CancellationToken ct) =>
        Task.CompletedTask;
    private static readonly string GuidPropertyName = ResolveGuidPropertyName();
    private static readonly string IdPropertyName = ResolveIdPropertyName();
    private static readonly PropertyInfo GuidPropertyInfo = typeof(TDto).GetProperty(GuidPropertyName)
        ?? throw new InvalidOperationException($"Type {typeof(TDto).Name} must expose a public GUID property.");
    private static readonly PropertyInfo IdPropertyInfo = typeof(TDto).GetProperty(IdPropertyName)
        ?? throw new InvalidOperationException($"Type {typeof(TDto).Name} must expose a public int ID property.");
    
    /// <summary>
    /// Attempts to load an existing DTO by GUID.
    /// </summary>
    public abstract Task<Result<TDto>> TryGetExistingDTO(Guid guid, string? content);
   
    /// <summary>
    /// Attempts to load an existing DTO by integer identifier.
    /// </summary>
    public async Task<Result<TDto>> TryGetExistingDTO(int id, string? content)
    {
        if (id <= 0)
            return Result<TDto>.Failure("Invalid dto id");

        var existing = await FilterById(DBSet.AsNoTracking(), id)
            .FirstOrDefaultAsync(CancellationToken.None);
        var success = existing is not null;
        return success
            ? Result<TDto>.SuccessResult(existing!)
            : Result<TDto>.Failure("No existing dto found");
    }


    /// <inheritdoc />
    public async Task<Result<IDto>> AddAsync(IDto dto, bool save, CancellationToken ct = default)
    {
        if (dto is not TDto tdto)
            return Result<IDto>.Failure("Invalid Dto type");

        try
        {
            await PrepareRelatedEntitiesAsync(tdto, ct).ConfigureAwait(false);
            // Optionally stamp server fields here if needed
            DBSet.Add(tdto); // tracked as Added

            if (save)
                await _context.SaveChangesAsync(ct);

            return Result<IDto>.SuccessResult(tdto);
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Add failed for {dto.Iguid}: {ex.GetBaseException().Message}");
            return Result<IDto>.Failure(ex.GetBaseException().Message);
        }
    }

    /// <inheritdoc />
    public async Task<Result<IDto>> UpdateAsync(IDto dto, bool save, CancellationToken ct = default)
    {
        if (dto is not TDto tdto)
            return Result<IDto>.Failure("Invalid Dto type");

        try
        {
            await PrepareRelatedEntitiesAsync(tdto, ct).ConfigureAwait(false);
            // Ensure we’re not double-tracking the same key
            var existing = await LocateExistingAsync(tdto, ct);
            if (existing is null)
                return Result<IDto>.Failure("DTO to update not found");

            EnsurePersistentKeyValues(tdto, existing);

            // Attach and mark modified (full replace pattern)
            _context.Attach(tdto);
            _context.Entry(tdto).State = EntityState.Modified;

            // If you want partial updates instead (safer), copy fields:
            // _context.Attach(localEntity);
            // localEntity.Name = tdto.Name; ... then Save (state stays Unchanged, EF detects changed members)

            if (save)
                await _context.SaveChangesAsync(ct);

            return Result<IDto>.SuccessResult(tdto);
        }
        catch (DbUpdateConcurrencyException cex)
        {
            _logger.LogError("DbContextHandler", $"Concurrency on update {dto.Iguid}: {cex.GetBaseException().Message}");
            return Result<IDto>.Failure("Concurrency conflict");
        }
        catch (Exception ex)
        {
            _logger.LogError("DbContextHandler", $"Update failed for {dto.Iguid}: {ex.GetBaseException().Message}");
            return Result<IDto>.Failure(ex.GetBaseException().Message);
        }
    }

 


    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<IDto>>> AddOrUpdateAsync(
        IReadOnlyList<IDto> dtos, bool save = true, CancellationToken ct = default)
    {
        if (dtos is null || dtos.Count == 0)
            return Result<IReadOnlyList<IDto>>.Failure("No dtos");

        var saved = new List<IDto>(dtos.Count);

        // Only create a transaction when we intend to save
        await using var tx = save ? await _context.Database.BeginTransactionAsync(ct) : null;

        // Speed up large batches
        var originalAutoDetect = _context.ChangeTracker.AutoDetectChangesEnabled;
        _context.ChangeTracker.AutoDetectChangesEnabled = false;

        try
        {
            foreach (var dto in dtos)
            {
                ct.ThrowIfCancellationRequested();

                var r = await AddOrUpdateAsync(dto, save: false, ct);
                if (!r.Success || r.Value is null)
                {
                    // Fail fast -> rollback whole batch for atomicity
                    var reason = r.ErrorMessage ?? "Unknown error";
                    return Result<IReadOnlyList<IDto>>.Failure($"Failed on DTO {dto.GetType().Name}: {reason}");
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

            return Result<IReadOnlyList<IDto>>.SuccessResult(saved);
        }
        catch (OperationCanceledException)
        {
            if (save && tx is not null) await tx.RollbackAsync(CancellationToken.None);
            throw;
        }
        catch (Exception ex)
        {
            if (save && tx is not null) await tx.RollbackAsync(CancellationToken.None);
            return Result<IReadOnlyList<IDto>>.Failure($"Batch add/update failed: {ex.GetBaseException().Message}");
        }
        finally
        {
            _context.ChangeTracker.AutoDetectChangesEnabled = originalAutoDetect;
        }
    }

    /// <summary>
    /// Filters the given queryable to match the provided GUID using the mapped store column.
    /// </summary>
    protected virtual IQueryable<TDto> FilterByGuid(IQueryable<TDto> query, Guid guid) =>
        query.Where(dto => EF.Property<Guid>(dto, GuidPropertyName) == guid);

    /// <summary>
    /// Filters the given queryable to match the provided integer identifier using the mapped store column.
    /// </summary>
    protected virtual IQueryable<TDto> FilterById(IQueryable<TDto> query, int id) =>
        query.Where(dto => EF.Property<int>(dto, IdPropertyName) == id);
    
    private static string ResolveGuidPropertyName()
    {
        var guidProp = typeof(TDto).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.PropertyType == typeof(Guid) &&
                                 (string.Equals(p.Name, "GUID", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(p.Name, "Guid", StringComparison.OrdinalIgnoreCase)));

        if (guidProp is null)
            throw new InvalidOperationException(
                $"Type {typeof(TDto).Name} must expose a public GUID property for filtering.");

        return guidProp.Name;
    }

    private static string ResolveIdPropertyName()
    {
        var idProp = typeof(TDto).GetProperties(BindingFlags.Instance | BindingFlags.Public)
            .FirstOrDefault(p => p.PropertyType == typeof(int) &&
                                 (string.Equals(p.Name, "ID", StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(p.Name, "Id", StringComparison.OrdinalIgnoreCase)));

        if (idProp is null)
            throw new InvalidOperationException(
                $"Type {typeof(TDto).Name} must expose a public int ID property for filtering.");

        return idProp.Name;
    }

    private async Task<TDto?> LocateExistingAsync(TDto dto, CancellationToken ct)
    {
        var query = DBSet.AsNoTracking();

        if (dto.Iid > 0)
        {
            var byId = await FilterById(query, dto.Iid).FirstOrDefaultAsync(ct);
            if (byId is not null)
                return byId;
        }

        if (dto.Iguid != Guid.Empty)
        {
            var byGuid = await FilterByGuid(query, dto.Iguid).FirstOrDefaultAsync(ct);
            if (byGuid is not null)
                return byGuid;
        }

        var content = dto.GetContent();
        if (!string.IsNullOrWhiteSpace(content))
        {
            var contentResult = await TryGetExistingDTO(Guid.Empty, content);
            if (contentResult.Success && contentResult.Value is not null)
                return contentResult.Value;
        }

        return null;
    }

    private static void EnsurePersistentKeyValues(TDto target, TDto existing)
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
    public async Task<Result<IDto>> AddOrUpdateAsync(IDto dto, bool save, CancellationToken ct = default)
    {
        _logger.Log("DbContextHandler", $"Trying to add or update dto {dto.Iguid}");

        if (dto is not TDto tdto)
        {
            _logger.LogWarning("DbContextHandler", $"Cant add or update dto {dto.Iguid} - wrong type");
            return Result<IDto>.Failure("Invalid Dto type");
        }

        var existing = await LocateExistingAsync(tdto, ct);
        if (existing is not null)
        {
            EnsurePersistentKeyValues(tdto, existing);

            if (!NeedUpdate(existing, tdto))
            {
                _logger.Log("DbContextHandler", $"No update needed for {dto.Iguid}");
                return Result<IDto>.SuccessResult(existing);
            }

            return await UpdateAsync(tdto, save, ct);
        }

        // Not found, add
        return await AddAsync(tdto, save, ct);
    }

    /// <summary>
    /// Return true if 'incoming' should overwrite 'existing'.
    /// Prefer server-authoritative stamps like UpdatedAtUtc/UpdatedSeq.
    /// </summary>
    private bool NeedUpdate(TDto existingDto, TDto incomingDto)
    {
        // if versioned DTOs
        if (existingDto is IVersionedDto ex && incomingDto is IVersionedDto inc)
        {
            // incoming is newer if timestamp is greater or same ts with higher seq
            return inc.UpdatedAtUtc > ex.UpdatedAtUtc
                   || (inc.UpdatedAtUtc == ex.UpdatedAtUtc && inc.UpdatedSeq > ex.UpdatedSeq);
        }

        //deep equality means no update
        if (existingDto.Equals(incomingDto)) return false;

        // Fallback: consider any difference as needing update
        return true;
    }
    // public async Task<Result<IDto>> AddAsync(IDto dto, bool save,CancellationToken ct = default)
    // {
    //     if (dto is not TDto tdto) return Result<IDto>.Failure("Invalid Dto type");
    //     return await AddAsync(tdto, save);
    // }

    // public async Task<Result<IDto>> UpdateAsync(IDto dto, bool save, CancellationToken ct = default)
    // {
    //     if (dto is not TDto tdto) return Result<IDto>.Failure("Invalid Dto type");;
    //    return await UpdateAsync(tdto, save, ct);
    // }

    public abstract Task<IReadOnlyList<IEntity>> GetAllEntityAsync(CancellationToken ct = default);


}
