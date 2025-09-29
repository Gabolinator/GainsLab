using System.Text.Json;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Infrastructure.DB.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace GainsLab.Infrastructure.DB.Outbox;

public class OutboxInterceptor : SaveChangesInterceptor
{
    public override ValueTask<InterceptionResult<int>> SavingChangesAsync(
        DbContextEventData eventData, 
        InterceptionResult<int> result, 
        CancellationToken ct = default)
    {
        var ctx = (GainLabSQLDBContext)eventData.Context!;
        var entries = ctx.ChangeTracker.Entries<BaseDto>()
            .Where(e => e.State is EntityState.Added or EntityState.Modified or EntityState.Deleted)
            .ToList();

        foreach (var e in entries)
        {
            if (e.State == EntityState.Modified) e.Entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            if (e.State == EntityState.Deleted)
            {
                // turn into soft delete
                e.State = EntityState.Modified;
                e.Entity.IsDeleted = true;
                e.Entity.DeletedAt = DateTimeOffset.UtcNow;
                e.Entity.UpdatedAtUtc = DateTimeOffset.UtcNow;
            }

            var envelope = new OutboxChangeDto
            {
                Entity = e.Entity.GetType().Name,
                EntityGuid = e.Entity.Iguid,
                ChangeType = e.Entity.IsDeleted ? ChangeType.Delete :
                    e.State == EntityState.Added ? ChangeType.Insert : ChangeType.Update,
                PayloadJson = JsonSerializer.Serialize(e.Entity)
            };

            ctx.Set<OutboxChangeDto>().Add(envelope);
        }

        return base.SavingChangesAsync(eventData, result, ct);
    }
}