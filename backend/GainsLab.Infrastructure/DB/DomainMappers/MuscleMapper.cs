using GainsLab.Core.Models.Core.CreationInfo;
using GainsLab.Core.Models.Core.Descriptor;
using GainsLab.Core.Models.Core.Entities.Descriptor;
using GainsLab.Core.Models.Core.Entities.Identifier;
using GainsLab.Core.Models.Core.Entities.WorkoutEntity;
using GainsLab.Core.Models.Core.Utilities;
using GainsLab.Infrastructure.DB.DTOs;
using GainsLab.Models.DataManagement.DB.Model.DomainMappers;

namespace GainsLab.Infrastructure.DB.DomainMappers;

public static class MuscleMapper
{
    public static MuscleDTO ToDTO(this MuscleEntity domain)
    {
        
      
        return new  MuscleDTO
        {
            GUID = domain.Id,
            Name = domain.Content.Name,
            BodySection = domain.Content.BodySection,
            CreatedAtUtc = domain.CreationInfo.CreatedAtUtc, // do i set to todays date ? 
            CreatedBy = domain.CreationInfo.CreatedBy,
            
            UpdatedAtUtc = domain.CreationInfo.UpdatedAtUtc?? CoreUtilities.Clock.UtcNow, // do i set to todays date ? 
            UpdatedBy = domain.CreationInfo.UpdatedBy,
            Version = domain.CreationInfo.Version,
             
            IsDeleted = domain.CreationInfo.IsDeleted,
            DeletedAt = domain.CreationInfo.DeletedAt,
            DeletedBy = domain.CreationInfo.DeletedBy,
            Descriptor = (DescriptorDTO)domain.Descriptor?.ToDTO(),
        };
    }

    /// <summary>
    /// Projects a descriptor DTO into the corresponding domain entity.
    /// </summary>
    /// <param name="dto">DTO retrieved from storage.</param>
    /// <returns>The reconstructed domain entity or <c>null</c> when the DTO is absent.</returns>
    public static MuscleEntity? ToDomain(this MuscleDTO? dto)
    {
        if (dto == null)
        {
            return null;
        }

        
        var descriptor =  (BaseDescriptorEntity)dto.Descriptor?.ToDomain();
        
        var content = new MuscleContent
        {
            Name = dto.Name,
            BodySection = dto.BodySection
        };

        var audited = new AuditedInfo(
            dto.CreatedAtUtc,
            dto.CreatedBy,
            dto.UpdatedAtUtc,
            dto.UpdatedBy,
            dto.Version,
            dto.IsDeleted,
            dto.DeletedAt,
            dto.DeletedBy);

        return new MuscleEntity(content, new MuscleId(dto.GUID),audited ,descriptor ?? new BaseDescriptorEntity(),dto.AntagonistGUIDs.Select(it=> new MuscleId(it)));

        

    }

    public static IEnumerable<MuscleAntagonistDTO> CreateMuscleAntagonistDTOs(
        IEnumerable<MuscleDTO> muscleDtos,
        IEnumerable<MuscleEntity> muscles)
    {
        if (muscleDtos == null) throw new ArgumentNullException(nameof(muscleDtos));
        if (muscles == null) throw new ArgumentNullException(nameof(muscles));

        var dtoById = muscleDtos
            .Where(dto => dto != null)
            .ToDictionary(dto => new MuscleId(dto.GUID));

        var links = new List<MuscleAntagonistDTO>();

        foreach (var muscle in muscles)
        {
            if (muscle == null || !muscle.AntagonistIds.Any()) continue;
            if (!dtoById.TryGetValue(muscle.Id, out var sourceDto)) continue;

            foreach (var antagonistId in muscle.AntagonistIds)
            {
                if (!dtoById.TryGetValue(antagonistId, out var antagonistDto)) continue;

                // Guard against duplicate links when the domain already contains them.
                var alreadyLinked = sourceDto.Antagonists
                    .Any(link => link.Antagonist.GUID == antagonistDto.GUID);
                if (alreadyLinked) continue;

                var link = new MuscleAntagonistDTO
                {
                    Muscle = sourceDto,
                    Antagonist = antagonistDto
                };

                sourceDto.Antagonists.Add(link);
                antagonistDto.Agonists.Add(link);
                links.Add(link);
            }
        }

        return links;
    }
}
