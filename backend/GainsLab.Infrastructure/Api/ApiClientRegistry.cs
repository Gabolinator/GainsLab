using GainsLab.Infrastructure.Api.Interface;

namespace GainsLab.Infrastructure.Api;

public class ApiClientRegistry : IApiClientRegistry
{
    
    public ApiClientRegistry(
        IDescriptorApi descriptorApi, 
        IEquipmentApi equipmentApi, 
        IMovementCategoryApi movementCategoryApi, 
        IMuscleApi muscleApi)
    {
        DescriptorApi = descriptorApi;
        EquipmentApi = equipmentApi;
        MovementCategoryApi = movementCategoryApi;
        MuscleApi = muscleApi;
    }

    public IDescriptorApi DescriptorApi { get; init; }
    public IEquipmentApi EquipmentApi { get; init; }
    public IMuscleApi MuscleApi { get; init; }
    public IMovementCategoryApi MovementCategoryApi { get; init; }
}
