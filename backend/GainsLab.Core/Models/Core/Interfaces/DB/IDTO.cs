using GainsLab.Models.Core;

namespace GainsLab.Models.DataManagement.DB.Model.DTOs;

public interface IDto
{
    public int Iid { get; }
    public Guid  Iguid { get;  }
}