using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Interfaces.DataManagement;
using GainsLab.Core.Models.Core.Interfaces.DB;
using GainsLab.Core.Models.Core.Results;

namespace GainsLab.Models.DataManagement.Sync.Processor;

public interface ISyncEntityProcessor
{
    EntityType EntityType { get; }                                                                                                                                 
    Task<Result> ApplyAsync(IReadOnlyList<ISyncDto> items, ILocalRepository localRepository, CancellationToken ct);    
}