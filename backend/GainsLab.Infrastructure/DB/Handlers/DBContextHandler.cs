using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Models.DataManagement.DB.Model.DTOs;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Handlers;

public abstract class IdbContextHandler<TDto> : IDBHandler where TDto : class, IDto
{
    protected IdbContextHandler(DbContext context, ILogger logger)
    {
        _context = context;
        _logger = logger;
    }

    public abstract DbSet<TDto> DBSet { get; }
    
    protected DbContext _context;
    protected readonly ILogger _logger;

    public abstract Task<Result<TDto>> TryGetExistingDTO(Guid guid);
   
    public async Task<Result<TDto>> TryGetExistingDTO(int id)
    {
        var existing = await DBSet
            .FirstOrDefaultAsync(e => e.Iid == id);
        var success = existing != null;
        return success ?Result<TDto>.SuccessResult(existing!) :  Result<TDto>.Failure("No existing dto found");
    }


    public async Task<Result<IDto>> AddAsync(IDto dto, bool save, CancellationToken ct = default)
    {
        if (dto is not TDto tdto)
            return Result<IDto>.Failure("Invalid Dto type");

        try
        {
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

    public async Task<Result<IDto>> UpdateAsync(IDto dto, bool save, CancellationToken ct = default)
    {
        if (dto is not TDto tdto)
            return Result<IDto>.Failure("Invalid Dto type");

        try
        {
            // Ensure we’re not double-tracking the same key
            var local = await DBSet.AsNoTracking().FirstOrDefaultAsync(x => x.Iguid == tdto.Iguid, ct);
            if (local is null)
                return Result<IDto>.Failure("DTO to update not found");

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
                await  _context.SaveChangesAsync(ct);
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

    public async Task<Result<IDto>> AddOrUpdateAsync(IDto dto, bool save, CancellationToken ct = default)
    {
        _logger.Log("DbContextHandler", $"Trying to add or update dto {dto.Iguid}");

        if (dto is not TDto tdto)
        {
            _logger.LogWarning("DbContextHandler", $"Cant add or update dto {dto.Iguid} - wrong type");
            return Result<IDto>.Failure("Invalid Dto type");
        }

    
        var existingResult = await TryGetExistingDTO(tdto.Iguid);
        if (existingResult.Success && existingResult.Value is TDto existing)
        {
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