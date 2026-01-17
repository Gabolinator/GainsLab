using System.Collections.Generic;
using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.GetDto;

/// <summary>
/// Response DTO describing a muscle, its descriptor metadata, and relationships.
/// </summary>
/// <param name="Id">Stable identifier of the muscle.</param>
/// <param name="Name">Display name.</param>
/// <param name="LatinName">Optional Latin/scientific name.</param>
/// <param name="BodySection">Body section classification.</param>
/// <param name="DescriptorId">Linked descriptor identifier.</param>
/// <param name="Descriptor">Descriptor payload projected with the muscle.</param>
/// <param name="AntagonistIds">Ids of muscles classified as antagonists.</param>
/// <param name="CreatedAtUtc">Creation timestamp tracked on the server.</param>
/// <param name="UpdatedAtUtc">Last update timestamp.</param>
/// <param name="UpdatedSeq">Monotonic sequence used for sync.</param>
/// <param name="IsDeleted">Marks soft-deleted records.</param>
/// <param name="Authority">Authority that owns the record.</param>
public sealed record MuscleGetDTO(
    Guid Id,
    string Name,
    string? LatinName,
    eBodySection BodySection,
    Guid? DescriptorId,
    DescriptorGetDTO? Descriptor,
    IReadOnlyList<Guid>? AntagonistIds,
    IReadOnlyList<MuscleRefDTO>? Antagonists,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional);


public sealed record MuscleRefDTO(Guid Id, string Name, string? LatinName ="");