using System.Text.Json;
using GainsLab.Application.Interfaces;
using GainsLab.Application.Results;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.PostDto;
using GainsLab.Contracts.Dtos.PutDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain.Interfaces;
using GainsLab.Infrastructure.DB.Context;

namespace GainsLab.Infrastructure.DB.Repository;


//for crud operation on descriptor
public class DescriptorRepository : IDescriptorRepository
{
    
    //note : no Delete() as its handled in cascading delete
    
    private readonly GainLabPgDBContext _db;
    private readonly IClock _clock;
    private readonly ILogger _log;
    
    
    private const string SyncActor = "repo";

    public DescriptorRepository(GainLabPgDBContext db, IClock clock, ILogger log)
    {
        _db = db;
        _clock = clock;
        _log = log;
    }
    
    public async Task<Result<DescriptorGetDTO>> PullByIdAsync(Guid id, CancellationToken ct)
    {
       
    }

    public async Task<Result<DescriptorPostDTO>> PostAsync(DescriptorPostDTO payload, CancellationToken ct)
    {
        
    }


    public async Task<Result<DescriptorPutDTO>> PutAsync(Guid id, DescriptorPutDTO payload, CancellationToken ct)
    {
        
    }

    public async Task<Result<DescriptorUpdateDTO>> PatchAsync(Guid id, DescriptorUpdateDTO payload, CancellationToken ct)
    {
      
    }
    
    
}