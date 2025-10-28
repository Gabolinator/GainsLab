using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core.Results;
using GainsLab.Core.Models.Core.Utilities.Logging;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Outbox;

/// <summary>
/// Very simple outbox dispatcher that marks local change envelopes as sent.
/// Actual transport to the server can be plugged in later.
/// </summary>
public sealed class OutboxDispatcher : IOutboxDispatcher
{
    private readonly IDbContextFactory<GainLabSQLDBContext> _dbContextFactory;
    private readonly ILogger _logger;

    /// <summary>
    /// Initializes a new dispatcher that accesses SQLite through an <see cref="IDbContextFactory{TContext}"/>.
    /// </summary>
    public OutboxDispatcher(IDbContextFactory<GainLabSQLDBContext> dbContextFactory, ILogger logger)
    {
        _dbContextFactory = dbContextFactory;
        _logger = logger;
    }

    /// <inheritdoc />
    public async Task<Result> DispatchAsync(CancellationToken ct)
    {
        try
        {
            await using var dbContext = await _dbContextFactory.CreateDbContextAsync(ct).ConfigureAwait(false);

            var pending = await dbContext.Set<OutboxChangeDto>()
                .Where(o => !o.Sent)
                .OrderBy(o => o.OccurredAt)
                .Take(100)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (pending.Count == 0)
                return Result.SuccessResult();

            foreach (var change in pending)
            {
                ct.ThrowIfCancellationRequested();
                _logger.Log(nameof(OutboxDispatcher),
                    $"Dispatching {change.ChangeType} for {change.Entity} ({change.EntityGuid})");
                change.Sent = true;
            }

            await dbContext.SaveChangesAsync(ct).ConfigureAwait(false);
            return Result.SuccessResult();
        }
        catch (OperationCanceledException)
        {
            return Result.Failure("Outbox dispatch cancelled");
        }
        catch (Exception ex)
        {
            _logger.LogError(nameof(OutboxDispatcher), $"Failed to dispatch outbox: {ex.Message}");
            return Result.Failure($"Failed to dispatch outbox: {ex.Message}");
        }
    }
}
