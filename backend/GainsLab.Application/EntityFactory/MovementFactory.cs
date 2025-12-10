using GainsLab.Application.Interfaces;
using GainsLab.Domain.Entities.CreationInfo;
using GainsLab.Domain.Entities.Descriptor;
using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Entities.WorkoutEntity;
using GainsLab.Domain.Interfaces;

namespace GainsLab.Application.EntityFactory;

public class MovementFactory
{
    private readonly IClock _clock;
    private readonly IDescriptorService<BaseDescriptorEntity> _descSvc;
    private readonly IEntitySeedResolver _resolver;

    public MovementFactory(IClock clock, IDescriptorService<BaseDescriptorEntity> descSvc, IEntitySeedResolver resolver)
    {
        _clock = clock;
        _descSvc = descSvc;
        _resolver = resolver;
    }

    public MovementEntity Create(
        MovementContent content,
        AuditedInfo auditedInfo,
        BaseDescriptorEntity descriptor,
        MovementPersistenceModel? persistence = null)
    {
        var id = MovementId.New();
        descriptor ??= _descSvc.CreateFor(id);
        var persistenceModel = persistence ?? MovementPersistenceModel.Empty;

        return new MovementEntity(content, id, auditedInfo, descriptor, persistence: persistenceModel);
    }
}


