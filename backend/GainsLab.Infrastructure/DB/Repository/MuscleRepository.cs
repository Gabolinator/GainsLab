using GainsLab.Application.DomainMappers;
using GainsLab.Application.DTOs.Extensions;
using GainsLab.Application.DTOs.Muscle;
using GainsLab.Application.Interfaces.DataManagement.Repository;
using GainsLab.Application.Results.APIResults;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Contracts.Dtos.UpdateDto.Outcome;
using GainsLab.Domain.Comparison;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;
using Microsoft.EntityFrameworkCore;

namespace GainsLab.Infrastructure.DB.Repository;

public class MuscleRepository : IMuscleRepository
{
    
    private readonly GainLabPgDBContext _db;
    private readonly IClock _clock;
    private readonly ILogger _log;
    
    
    private const string SyncActor = "repo";
    
    public MuscleRepository(GainLabPgDBContext db, IClock clock, ILogger log)
    {
        _db = db;
        _clock = clock;
        _log = log;
    }
    
    public async Task<APIResult<MuscleGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
        if (id == Guid.Empty) return APIResult<MuscleGetDTO>.BadRequest("Id cannot be empty");

        try
        {
            var entry = await _db.Muscles
                .Where(c=>!c.IsDeleted)
                .Include(m=>m.Descriptor)
                .Include(m=>m.Antagonists)
                .AsNoTracking()
                .FirstOrDefaultAsync(m=>m.GUID ==id,ct);
            
            return entry !=null ? 
                APIResult<MuscleGetDTO>.Found(entry.ToGetDTO()!):
                APIResult<MuscleGetDTO>.NotFound(id.ToString());
        }
        
        catch (OperationCanceledException)
        {
            return APIResult<MuscleGetDTO>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MuscleGetDTO>.Exception(e.Message);
        }
    }

    public async Task<APIResult<MuscleGetDTO>> PostAsync(MusclePostDTO payload, CancellationToken ct)
    {
        if(payload ==null) return APIResult<MuscleGetDTO>.BadRequest("Payload cannot be null");
        if(payload.Id == Guid.Empty) return APIResult<MuscleGetDTO>.BadRequest("Payload ID cannot be empty");

        try
        {
            _log.Log(nameof(MuscleRepository), $"Try to post {payload.Print()}");
            
            var entity = payload.ToEntity(_clock);
            if (entity == null)
            {
                return APIResult<MuscleGetDTO>.BadRequest("Unable to convert payload to entity");
            }

            // Deduplicate antagonist IDs and prevent accidental self-references.
            var antagonistGuids = payload.AntagonistIds?
                .Where(g => g != Guid.Empty && g != payload.Id)
                .Distinct()
                .ToList();

            _log.Log(nameof(MuscleRepository), $"Post Muscle {entity.Id} {entity.Name} {entity.BodySection}");

            
            // Insert the muscle (and descriptor) first so it receives its database id.
            var entry = await _db.Muscles.AddAsync(entity, ct).ConfigureAwait(false);
            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            if (antagonistGuids is { Count: > 0 })
            {
                // Resolve existing antagonists from the database.
                var antagonists = await _db.Muscles
                    .AsNoTracking()
                    .Where(m => antagonistGuids.Contains(m.GUID) && !m.IsDeleted)
                    .Select(m => new { m.GUID, m.Id })
                    .ToListAsync(ct)
                    .ConfigureAwait(false);

                var resolvedIds = antagonists.ToDictionary(x => x.GUID, x => x.Id);
                var missing = antagonistGuids.Except(resolvedIds.Keys).ToList();
                if (missing.Count > 0)
                {
                    _log.LogWarning(nameof(MuscleRepository),
                        $"Antagonists not found for muscle {payload.Id}: {string.Join(", ", missing)}");
                }

                if (resolvedIds.Count > 0)
                {
                    var antagonistIdSet = resolvedIds.Values.ToHashSet();
                    var existingRelationIds = await _db.MuscleAntagonists
                        .Where(link => link.MuscleId == entry.Entity.Id && antagonistIdSet.Contains(link.AntagonistId))
                        .Select(link => link.AntagonistId)
                        .ToListAsync(ct)
                        .ConfigureAwait(false);
                    var existingSet = existingRelationIds.ToHashSet();

                    var newRelations = new List<MuscleAntagonistRecord>();
                    foreach (var (guid, antagonistId) in resolvedIds)
                    {
                        if (existingSet.Contains(antagonistId))
                        {
                            _log.Log(nameof(MuscleRepository),
                                $"Reusing existing muscle-antagonist relation for {payload.Id} <-> {guid}");
                            continue;
                        }

                        newRelations.Add(new MuscleAntagonistRecord
                        {
                            MuscleId = entry.Entity.Id,
                            AntagonistId = antagonistId
                        });
                    }

                    if (newRelations.Count > 0)
                    {
                        await _db.MuscleAntagonists.AddRangeAsync(newRelations, ct).ConfigureAwait(false);
                        await _db.SaveChangesAsync(ct).ConfigureAwait(false);
                    }
                }
            }

            var created = await _db.Muscles
                .AsNoTracking()
                .Where(m => m.Id == entry.Entity.Id)
                .Include(m => m.Descriptor)
                .Include(m => m.Antagonists)
                    .ThenInclude(link => link.Antagonist)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            if (created == null)
            {
                return APIResult<MuscleGetDTO>.Problem("Failed to load created muscle");
            }

            return APIResult<MuscleGetDTO>.Created(created.ToGetDTO()!);
            
        }

        catch (OperationCanceledException)
        {
            return  APIResult<MuscleGetDTO>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
           return  APIResult<MuscleGetDTO>.Exception(e.Message);
        }

    }

    public async Task<APIResult<MusclePutDTO>> PutAsync(Guid id, MusclePutDTO payload, CancellationToken ct)
    {
       throw new NotImplementedException();
    }

    public async Task<APIResult<MuscleUpdateOutcome>> PatchAsync(Guid id, MuscleUpdateDTO payload, CancellationToken ct)
    {
        if(id == Guid.Empty) return APIResult<MuscleUpdateOutcome>.BadRequest("Payload ID cannot be empty");

        try
        {
            var existing = _db.Muscles
                .Where(c=> !c.IsDeleted)
                .Include(m=>m.Descriptor)
                .Include(m=>m.Antagonists)
                    .ThenInclude(link => link.Antagonist)
                .FirstOrDefault(c => c.GUID == id);
            
            if(existing is null) return APIResult<MuscleUpdateOutcome>.NotFound($"Could not find existing muscle with id: {id}");

           
            // Deduplicate antagonist IDs and prevent accidental self-references.
            var antagonistGuids = payload.AntagonistIds?
                .Where(g => g != Guid.Empty && g != id)
                .Distinct()
                .ToList();
            
            var existingGuid =existing.AntagonistGUIDs?
                .Where(g => g != Guid.Empty && g != id)
                .Distinct()
                .ToList();
            
           
            bool antagonistChanged = !SequenceComparison.SetEqualWithDiff(
                antagonistGuids,
                existingGuid,
                g => g,
                out SetDifference<Guid> difference
            );
            
            var antagonistToAdd= antagonistChanged ? difference.ToAdd.ToList() : new List<Guid>();
            var antagonistToRemove  = antagonistChanged ? difference.ToRemove.ToList() : new List<Guid>();
                

            //handle the adding of new antagonist relations
            if (antagonistToAdd.Any())
            {
                await AddNewAntagonistsRelations(existing, antagonistToAdd, ct);
            }
            else _log.Log(nameof(MuscleRepository),"No new Antagonists to add");
            
            //handle the removing of new antagonist relations
            if (antagonistToRemove.Any())
            {
                await RemoveAntagonistsRelations(existing, antagonistToRemove, ct);
            }
            else _log.Log(nameof(MuscleRepository),"No Antagonists to remove");
                
            var muscleContentChanged = existing.TryUpdate(payload, _clock, _log) || antagonistChanged;
            
            var descriptorOutcomeState = payload.Descriptor == null ? UpdateOutcome.NotRequested : UpdateOutcome.NotUpdated;
            var descriptorChanged = false;
            DescriptorUpdateOutcome? descriptorOutcome = null;

            if (payload.Descriptor != null)
            {
                //not sure it make sense to make movement category depend on description
                if (existing.Descriptor == null)
                {
                    _log.LogWarning(nameof(MuscleRepository), $"No descriptor provided");

                    return APIResult<MuscleUpdateOutcome>.Problem("Descriptor missing for movement category");
                }

                descriptorChanged = existing.Descriptor.TryUpdate(payload.Descriptor, _clock);
                descriptorOutcomeState = descriptorChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated;
                descriptorOutcome = new DescriptorUpdateOutcome(descriptorOutcomeState, existing.Descriptor.ToGetDTO());
            }

            if (!muscleContentChanged && !descriptorChanged)
            {
                return APIResult<MuscleUpdateOutcome>.NothingChanged("Nothing changed");
            }

            await _db.SaveChangesAsync(ct).ConfigureAwait(false);

            var updatedRecord = await _db.Muscles
                .AsNoTracking()
                .Where(m => m.Id == existing.Id)
                .Include(m => m.Descriptor)
                .Include(m => m.Antagonists)
                    .ThenInclude(link => link.Antagonist)
                .FirstOrDefaultAsync(ct)
                .ConfigureAwait(false);

            var updatedState = updatedRecord?.ToGetDTO();
            var outcome = new MuscleUpdateOutcome(
                muscleContentChanged ? UpdateOutcome.Updated : UpdateOutcome.NotUpdated,
                descriptorOutcomeState,
                descriptorOutcome,
                updatedState);

            return APIResult<MuscleUpdateOutcome>.Updated(outcome);



        }
        
        
        catch (OperationCanceledException)
        {
            return APIResult<MuscleUpdateOutcome>.Exception("Operation was canceled");
        }
        catch (Exception e)
        {
            return APIResult<MuscleUpdateOutcome>.Exception(e.Message);
        }

    }

    private async Task RemoveAntagonistsRelations(MuscleRecord existing, List<Guid> antagonistToRemove, CancellationToken ct)
    {
        var toRemoveRecords = existing.Antagonists
            .Where(link => link.Antagonist != null && antagonistToRemove.Contains(link.Antagonist.GUID))
            .ToList();

        if (toRemoveRecords.Count == 0)
        {
            var antagonistIds = await _db.Muscles
                .Where(m => antagonistToRemove.Contains(m.GUID) && !m.IsDeleted)
                .Select(m => m.Id)
                .ToListAsync(ct)
                .ConfigureAwait(false);

            if (antagonistIds.Count > 0)
            {
                toRemoveRecords = await _db.MuscleAntagonists
                    .Where(link => link.MuscleId == existing.Id && antagonistIds.Contains(link.AntagonistId))
                    .ToListAsync(ct)
                    .ConfigureAwait(false);
            }
        }

        if (toRemoveRecords.Count > 0)
        {
            _db.MuscleAntagonists.RemoveRange(toRemoveRecords);
            foreach (var relation in toRemoveRecords)
            {
                existing.Antagonists.Remove(relation);
            }

            _log.Log(nameof(MuscleRepository),
                $"Removed {toRemoveRecords.Count} antagonist relations for {existing.GUID}");
        }
    }

    private async Task AddNewAntagonistsRelations(
        MuscleRecord existing, 
        List<Guid> antagonistToAdd, 
        CancellationToken ct)
    {
        var resolvedAntagonists = await _db.Muscles
            .Where(m => antagonistToAdd.Contains(m.GUID) && !m.IsDeleted)
            .Select(m => new { m.GUID, m.Id })
            .ToListAsync(ct)
            .ConfigureAwait(false);

        var resolved = resolvedAntagonists.ToDictionary(x => x.GUID, x => x.Id);
        var missing = antagonistToAdd.Where(g => !resolved.ContainsKey(g)).ToList();
        if (missing.Count > 0)
        {
            _log.LogWarning(nameof(MuscleRepository),
                $"Unable to resolve antagonists for {existing.GUID}: {string.Join(", ", missing)}");
        }

        var newRelations = new List<MuscleAntagonistRecord>();
        foreach (var guid in antagonistToAdd)
        {
            if (!resolved.TryGetValue(guid, out var antagonistId))
            {
                continue;
            }

            newRelations.Add(new MuscleAntagonistRecord
            {
                MuscleId = existing.Id,
                AntagonistId = antagonistId
            });
        }

        if (newRelations.Count > 0)
        {
            await _db.MuscleAntagonists.AddRangeAsync(newRelations, ct).ConfigureAwait(false);
            foreach (var relation in newRelations)
            {
                existing.Antagonists.Add(relation);
            }

            _log.Log(nameof(MuscleRepository),
                $"Added {newRelations.Count} antagonist relations for {existing.GUID}");
        }
    }


    public async Task<APIResult<MuscleGetDTO>> DeleteAsync(Guid id, CancellationToken ct)
    {
      if(id == Guid.Empty) return APIResult<MuscleGetDTO>.BadRequest("Payload ID cannot be empty");

      _log.Log(nameof(MuscleRepository),$"Trying to delete muscle with id {id})");

      
      try
      {
            
          var existing = await _db.Muscles
              .Where(c=>!c.IsDeleted)
              .Include(m=>m.Descriptor)
              .Include(m=>m.Antagonists)
                  .ThenInclude(link => link.Antagonist)
              .AsNoTracking()
              .FirstOrDefaultAsync(m=>m.GUID ==id,ct);

          if (existing is null)
          {
              return APIResult<MuscleGetDTO>.NotFound($"Could not Delete muscle with id {id} - Not Found");
          }
          
          
          var dto = existing.ToGetDTO();
          _log.Log(nameof(MuscleRepository),$"Deleted {existing.Name} with descriptor id : {(existing.Descriptor != null ? existing.Descriptor.Iguid : "null")}");
            
          _db.Muscles.Remove(existing);
          await _db.SaveChangesAsync(ct).ConfigureAwait(false);

          return APIResult<MuscleGetDTO>.Deleted(dto!);
          
      }
      catch (OperationCanceledException)
      {
          return APIResult<MuscleGetDTO>.Exception("Operation was canceled");
      }
      catch (Exception e)
      {
          return APIResult<MuscleGetDTO>.Exception(e.Message);
      }
    }
}
