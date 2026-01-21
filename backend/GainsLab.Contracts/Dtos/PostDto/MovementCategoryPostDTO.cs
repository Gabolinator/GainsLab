using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.PostDto;

/// <summary>
/// Payload used to create a movement category and its descriptor metadata.
/// </summary>
public record MovementCategoryPostDTO
{
    [Required]
    public Guid Id { get; init; }

    [Required]
    [StringLength(256, MinimumLength = 2)]
    public string Name { get; init; } = string.Empty;

    public Guid? ParentCategoryId { get; init; }

    [Required]
    public IReadOnlyList<eMovementCategories> BaseCategories { get; init; } =
        Array.Empty<eMovementCategories>();

    public DescriptorPostDTO? Descriptor { get; init; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority Authority { get; init; } = DataAuthority.Bidirectional;

    public string? CreatedBy { get; init; }
}

public static class MovementCategoryPostDTOExtensions
{
    public static string Print(this MovementCategoryPostDTO  entity)
    {
        if (entity is null)
        {
            return "MovementCategoryPostDTO <null>";
        }

        var baseCategories = entity.BaseCategories is { Count: > 0 }
            ? string.Join(", ", entity.BaseCategories)
            : "none";

        var descriptor = entity.Descriptor is null
            ? "null"
            : $"{{ Id: {entity.Descriptor.Id}, DescriptionContent: \"{entity.Descriptor.DescriptionContent}\", Notes: \"{entity.Descriptor.Notes ?? "null"}\", Tags: [{(entity.Descriptor.Tags is { Count: > 0 } tags ? string.Join(", ", tags) : "none")}], Authority: {entity.Descriptor.Authority}, CreatedBy: {entity.Descriptor.CreatedBy ?? "null"} }}";

        return
            $"MovementCategoryPostDTO {{ Id: {entity.Id}, Name: \"{entity.Name}\", ParentCategoryId: {(entity.ParentCategoryId.HasValue ? entity.ParentCategoryId.ToString() : "null")}, BaseCategories: [{baseCategories}], Descriptor: {descriptor}, Authority: {entity.Authority}, CreatedBy: {entity.CreatedBy ?? "null"} }}";
    }
}
