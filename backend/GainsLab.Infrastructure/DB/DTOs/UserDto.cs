using System.ComponentModel.DataAnnotations;
using GainsLab.Core.Models.Core;
using GainsLab.Core.Models.Core.Entities.User;
using GainsLab.Models.Core.User;

namespace GainsLab.Infrastructure.DB.DTOs;

public class UserDto : BaseDto
{

    [Key] public int Id { get; set; } //primary - auto increment

    public string Name { get; set; } //user name

    public Guid GUID { get; set; }

    public AccountRole Role { get; set; }

    public string Permissions { get; set; } //list of int separated by '-'

    public SubscriptionInfos.SubscriptionType SubscriptionType { get; set; }

    public override string? GetContent()
    {
        //make sure we capture the proper information so we dont duplicate entry
        //right now just return the name 
        return Name;
    }

     public override int Iid => Id;
     public override  Guid Iguid => GUID;
     public override EntityType Type => EntityType.User;

     public override bool Equals(object? obj)
    {
        if (obj is not EquipmentDTO other) return false;
        return Guid.Equals(GUID,other.GUID);
    }

    public override int GetHashCode() => HashCode.Combine(GUID);

}