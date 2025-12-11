


namespace GainsLab.Models.DataManagement.FileAccess;

public static class ComponentJsonHelper
{
    // private static readonly JsonSerializerOptions _options = new JsonSerializerOptions
    // {
    //     TypeInfoResolver = new DefaultJsonTypeInfoResolver
    //     {
    //         Modifiers =
    //         {
    //             typeInfo =>
    //             {
    //                 //todo add all types 
    //                 if (typeInfo.Type == typeof(IWorkoutComponent))
    //                 {
    //                     typeInfo.PolymorphismOptions = new JsonPolymorphismOptions
    //                     {
    //                         TypeDiscriminatorPropertyName = "$type",
    //                         DerivedTypes =
    //                         {
    //                             new JsonDerivedType(typeof(Equipment), "equipment"),
    //                             new JsonDerivedType(typeof(Muscle), "muscle")
    //                         }
    //                     };
    //                 }
    //             }
    //         }
    //     },
    //     WriteIndented = true
    // };
    //
    // public static string ToJson(this IWorkoutComponent component)
    //     => JsonSerializer.Serialize(component, _options);
    //
    // public static IWorkoutComponent? FromJson(string json)
    //     => JsonSerializer.Deserialize<IWorkoutComponent>(json, _options);
}