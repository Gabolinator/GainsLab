using System.ComponentModel.DataAnnotations;
using System.Xml;
using GainsLab.Contracts;
using GainsLab.Contracts.Dtos.GetDto;
using GainsLab.Contracts.Dtos.UpdateDto;
using GainsLab.Domain;

namespace GainsLab.WebLayer.Model;

public class DescriptorEditDTO
{
    public Guid Id { get; set; }

    [StringLength(1024, MinimumLength = 3)]
    public string? DescriptionContent { get; set; } = "new description";

    [StringLength(2048)]
    public string? Notes { get; set; }

    [MaxLength(20)]
    public ICollection<string>? Tags { get; set; }

    [EnumDataType(typeof(DataAuthority))]
    public DataAuthority? Authority { get; set; }

    public string? UpdatedBy { get; set; }

    public UpdateRequest UpdateRequest { get; set; } = UpdateRequest.DontUpdate;
}


public static class DescriptorEditDTOExtensions
{
    public static DescriptorEditDTO FromGetDTO(this DescriptorGetDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new DescriptorEditDTO
        {
            Id =  dto.Id,
            DescriptionContent = dto.content,
            Notes = null,
            Tags = null,
            Authority = dto.Authority,
            UpdatedBy = null
        };
    }

    public static DescriptorUpdateDTO ToUpdateDTO(this DescriptorEditDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));

        return new DescriptorUpdateDTO
        {
            DescriptionContent = dto.DescriptionContent,
            Notes = dto.Notes,
            Tags = dto.Tags?.ToList(),
            Authority = dto.Authority,
            UpdatedBy = dto.UpdatedBy
        };
    }
    
    public static DescriptorUpdateRequest ToUpdateRequest(this DescriptorEditDTO dto)
    {
        if (dto is null) throw new ArgumentNullException(nameof(dto));
        
        return new DescriptorUpdateRequest(dto.ToUpdateDTO(), dto.Id, dto.UpdateRequest ,dto.UpdatedBy ?? "unknown");
        
    }
}