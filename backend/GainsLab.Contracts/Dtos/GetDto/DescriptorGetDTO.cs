using GainsLab.Domain;

namespace GainsLab.Contracts.Dtos.GetDto;

public record DescriptorGetDTO(
    Guid Id, 
    string Content, 
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    long UpdatedSeq,
    bool IsDeleted = false,
    DataAuthority Authority = DataAuthority.Bidirectional );