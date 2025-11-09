# Extending GainsLab With New Sync Entities

This guide distills the steps from the existing equipment/descriptor implementations so you can introduce future entities (e.g., `Muscle`, `Workout`, `Movement`) without missing any layers. Follow the sections in order; each builds on shared abstractions described in the project READMEs.

---

## 1. Plan the Domain Slice

Before touching code:

1. Define the source of truth (upstream server, downstream editor, or both) so you can set the initial `DataAuthority`.
2. Decide what constitutes the natural key (typically a GUID) and which related entities you need to load during sync.

---

## 2. Core Layer (`backend/GainsLab.Core`)

1. **Domain types** – add any identifiers, value objects, or aggregates required by the entity under `Models/Core/Entities`.
2. **Interfaces** – extend shared contracts if the new entity needs special processing (e.g., caching hooks). Most sync types only require `ISyncDto`.
3. **DataAuthority defaults** – if the new entity has unique ownership rules, update `DataAuthority` or introduce helper methods so other layers can infer defaults.

---

## 3. Infrastructure Layer (`backend/GainsLab.Infrastructure`)

### DTOs
1. Create a new DTO class under `DB/DTOs` inheriting from `BaseDto`.
2. Add navigation properties for related records (e.g., `DescriptorDTO`). Remember `BaseDto` already provides `Authority`, `UpdatedAtUtc`, `IsDeleted`, etc.

### DbContexts
3. Register the DTO:
   - Add `DbSet<YourDto>` to both `GainLabPgDBContext` and `GainLabSQLDBContext`.
   - Configure table metadata inside `Create*TableModel` methods (schema, foreign keys, unique indices, `authority` column defaulting to `DataAuthority.Bidirectional`).

### Migrations
4. Add EF Core migrations for **both** contexts:
   ```bash
   dotnet ef migrations add AddYourEntity \
     --project backend/GainsLab.Infrastructure \
     --startup-project backend/GainsLab.Api \
     --context GainLabPgDBContext

   dotnet ef migrations add AddYourEntityLocal \
     --project backend/GainsLab.Infrastructure \
     --startup-project desktop/GainsLab.Editor \
     --context GainLabSQLDBContext \
     --output-dir Migrations/GainLabSQLDB
   ```
5. Review the generated migration code to ensure `authority` has the right default and index coverage matches your sync ordering (`UpdatedAtUtc`, `UpdatedSeq`).

### Seeders
6. Update `DBDataInitializer` (or other seed helpers) if you need baseline rows on first boot.

---

## 4. Contracts Layer (`backend/GainsLab.Contracts`)

### Sync DTOs
1. Create `YourEntitySyncDTO` under `SyncDto/` with the standard signature:
   ```csharp
   public record YourEntitySyncDTO(
       Guid GUID,
       ... // payload fields
       DateTimeOffset UpdatedAtUtc,
       long UpdatedSeq,
       bool IsDeleted = false,
       DataAuthority Authority = DataAuthority.Bidirectional) : ISyncDto;
   ```

### Mapper
2. Add a mapper similar to `EquipmentSyncMapper` that converts between EF DTOs and sync DTOs while copying `Authority`, timestamps, and tombstone metadata.

### Sync Service
3. Implement `YourEntitySyncService : ISyncService<YourEntitySyncDTO>`:
   - `PullAsync` should order by `UpdatedAtUtc` + `UpdatedSeq` and include related entities via `Include`.
   - `PushAsync` validates GUIDs, fetches existing rows by GUID, enforces `DataAuthority`, updates row fields, stamps server metadata, and writes outbox envelopes.
   - Use `NextUpdateSeqAsync` (or similar) to obtain monotonically increasing sequence numbers.

4. Update the contracts README if necessary to mention the new DTO.

---

## 5. API Layer (`backend/GainsLab.Api`)

1. Register the new service in `Program.ConfigureServices`:
   ```csharp
   services.AddScoped<ISyncService<YourEntitySyncDTO>, YourEntitySyncService>();
   services.AddScoped<ISyncService>(sp => sp.GetRequiredService<ISyncService<YourEntitySyncDTO>>());
   ```

2. If the controller enumerates supported entity types, ensure your `EntityType` enum includes the new value and the controller can resolve it.

3. No additional controller code is required if the existing sync endpoints are generic over `ISyncService`.

---

## 6. Desktop Editor (`desktop/GainsLab.Editor`)

### DTO Persistence
1. Ensure the local SQLite context includes the DTO (already handled in step 3).

### Sync Processor
2. Implement `YourEntitySyncProcessor : ISyncEntityProcessor` mirroring the pattern used by descriptors/equipment:
   - Resolve or create related rows (e.g., descriptors).
   - Upsert into SQLite with local timestamps and `Authority`.
   - Return `Result.SuccessResult()` so the orchestrator continues the batch.

3. Register the processor in `ServiceConfig` and make sure the orchestrator enumerates it.

### UI + Caches
4. Hook the data into existing caches or create a new cache if the UI requires quick lookups.

5. Update view models to respect `Authority` (e.g., disable editing for upstream-owned rows).

---

## 7. Outbox & Sync Safety Nets

1. The `OutboxInterceptor` already normalizes payloads for deduplication. If your new entity adds fields that should be ignored for dedup (e.g., additional timestamps), append them to `DedupIgnoredProperties`.
2. Verify that server push services log clear reasons when `DataAuthority` rejects a change; mirror that behavior on the editor side to avoid confusing users.

---

## 8. End-to-End Checklist

- [ ] Domain types and enums updated.
- [ ] Infrastructure DTO + contexts + migrations (Postgres & SQLite).
- [ ] Seed data (optional) updated.
- [ ] Sync DTO + mapper created with `Authority`.
- [ ] Sync service implemented and registered.
- [ ] Editor sync processor implemented and registered.
- [ ] Outbox dedup ignores irrelevant fields (if needed).
- [ ] README / docs mention the new entity where appropriate.
- [ ] Databases migrated or reseeded.
- [ ] Manual sync test: push from editor, pull from server, observe outbox + authority behavior.

Following this sequence keeps schema, contracts, and runtime behavior in lockstep, making it easier to onboard future entities without rediscovering the wiring each time.
