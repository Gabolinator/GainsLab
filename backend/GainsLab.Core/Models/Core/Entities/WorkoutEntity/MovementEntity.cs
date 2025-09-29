using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Interfaces.Entity;
using GainsLab.Models.Core;


namespace GainsLab.Core.Models.Core.Entities.WorkoutEntity;

public sealed record MovementContent(
    string Name,
    MovementCategoryId Category,
    MuscleWorked MusclesWorked,
    EquipmentIdList EquipmentRequired
) : IEntityContent<MovementContent>
{
    public MovementContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name)) throw new ArgumentException("Movement name is required.", nameof(Name));
        return this;
    }
}

    
    public sealed class MovementEntity
        : EntityBase<MovementId, MovementContent, AuditedInfo>,
            IDescribed<BaseDescriptorEntity>
    {
        public BaseDescriptorEntity Descriptor { get; private set; }

       
        public MovementEntity(MovementContent content, string createdBy, BaseDescriptorEntity? descriptor = null, int dbId = -1)
            : base(MovementId.New(), content.Validate(), AuditedInfo.New(createdBy), dbId)
        {
            Descriptor = descriptor ?? new BaseDescriptorEntity();
        }

        public override EntityType Type => EntityType.Movement;
        private MovementEntity() { Descriptor = new BaseDescriptorEntity(); }


        public MovementEntity Rename(string newName)
            =>WithContent((Content with { Name = newName }).Validate());

        public MovementEntity Recat(MovementCategoryId newCategory)
            => WithContent(Content with { Category = newCategory });

        public MovementEntity WithMuscles(MuscleWorked muscles)
            => WithContent(Content with { MusclesWorked = muscles });

        public MovementEntity WithEquipment(EquipmentIdList equipment)
            => WithContent(Content with { EquipmentRequired = equipment });

        public MovementEntity WithDescriptor(BaseDescriptorEntity descriptor)
            => new() {
                Id = this.Id,
                Content = this.Content,
                CreationInfo = this.CreationInfo,
                Descriptor = descriptor
            };

        private MovementEntity WithContent(MovementContent newContent)
            => new() {
                Id = this.Id,
                Content = newContent,
                CreationInfo = this.CreationInfo,
                Descriptor = this.Descriptor
            };
    
}