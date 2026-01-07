namespace GainsLab.Infrastructure.Api.Interface;

public interface IApiClientRegistry
{
    public IDescriptorApi DescriptorApi  { get; init; }
    public IEquipmentApi EquipmentApi { get; init; }
    
}