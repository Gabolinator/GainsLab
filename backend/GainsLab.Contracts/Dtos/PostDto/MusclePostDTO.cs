using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PostDto;

/// <summary>
/// Payload used to create a new muscle along with descriptor metadata.
/// </summary>
public record MusclePostDTO
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    [StringLength(256)]
    public string? LatinName { get; init; }

    [Required]
    [EnumDataType(typeof(eBodySection))]
    public eBodySection BodySection { get; init; } = eBodySection.undefined;

    public DescriptorPostDTO? Descriptor { get; init; }

    public IReadOnlyList<Guid>? AntagonistIds { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; init; } = DataAuthority.Bidirectional;

    public string? CreatedBy { get; init; }
}

public static class MusclePostDTOExtensions
{
    public static string Print(this MusclePostDTO entity)
    {
        if (entity is null)
        {
            return "MusclePostDTO <null>";
        }

        var antagonists = entity.AntagonistIds is { Count: > 0 }
            ? string.Join(", ", entity.AntagonistIds)
            : "none";

        var descriptor = entity.Descriptor is null
            ? "null"
            : $"{{ Id: {entity.Descriptor.Id}, DescriptionContent: \"{entity.Descriptor.DescriptionContent}\", Notes: \"{entity.Descriptor.Notes ?? "null"}\", Tags: [{(entity.Descriptor.Tags is { Count: > 0 } tags ? string.Join(", ", tags) : "none")}], Authority: {entity.Descriptor.Authority}, CreatedBy: {entity.Descriptor.CreatedBy ?? "null"} }}";

        return
            $"MusclePostDTO {{ Id: {entity.Id}, Name: \"{entity.Name}\", BodySection  {entity.BodySection} Antagonists: [{antagonists}], Descriptor: {descriptor}, Authority: {entity.Authority}, CreatedBy: {entity.CreatedBy ?? "null"} }}";

    }
}

