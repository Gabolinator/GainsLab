using GainsLab.Infrastructure.Api.Interface;

namespace GainsLab.Infrastructure.Api;

public class ApiClientRegistry : IApiClientRegistry
{
    
    public ApiClientRegistry(IDescriptorApi descriptorApi, IEquipmentApi equipmentApi)
    {
        DescriptorApi = descriptorApi;
        EquipmentApi = equipmentApi;
    }

    public IDescriptorApi DescriptorApi { get; init; }
    public IEquipmentApi EquipmentApi { get; init; }
   
}
