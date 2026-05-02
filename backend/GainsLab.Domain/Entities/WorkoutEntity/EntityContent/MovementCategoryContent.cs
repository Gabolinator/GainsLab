using GainsLab.Domain.Entities.Identifier;
using GainsLab.Domain.Interfaces.Entity;

namespace GainsLab.Domain.Entities.WorkoutEntity.EntityContent;

/// <summary>
/// Immutable details describing a movement category and its base categorizations.
/// </summary>
public record MovementCategoryContent(string Name , params eMovementCategories[] BaseCategories) : IEntityContent<MovementCategoryContent>
{
    public MovementCategoryId? ParentCategoryId { get; set; } = null;
    
    public MovementCategoryContent Validate()
    {
        if (string.IsNullOrWhiteSpace(Name))
            throw new ArgumentException("MovementCategory name is required.", nameof(Name));

        if (BaseCategories is null || BaseCategories.Length == 0)
            throw new ArgumentException($" Movemenent Category: {Name} : At least one base category is required.", nameof(BaseCategories));

        if (BaseCategories.Contains(eMovementCategories.undefined))
            throw new ArgumentException($"Base categories of {Name} cannot include 'undefined'.", nameof(BaseCategories));

        return this;
    }
}