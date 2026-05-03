using GainsLab.Domain;
using GainsLab.Domain.Entities.Identifier;

namespace GainsLab.Contracts.Dtos.GetDto;

public record DescriptorGetDTO(
    DescriptorId Id, 
    string Content, 
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional );