using GainsLab.Application.Interfaces.DataManagement;
using GainsLab.Application.Results;
using GainsLab.Contracts.Interface;
using GainsLab.Domain;


namespace GainsLab.Infrastructure.Sync.Processor;

public interface ISyncEntityProcessor
{
    EntityType EntityType { get; }                                                                                                                                 
    Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct);    
}