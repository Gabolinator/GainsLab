namespace GainsLab.Domain;

public static class EnumExtensions
{
    public static string GetDescription(this eMovementCategories category)
    {
        string descBase = "define me - some base description for: ";

        return descBase + category;

        // return category switch
        // {
        //     eMovementCategories.BodyWeight => descBase+category,
        //     eMovementCategories.Weightlifting => expr,
        //     eMovementCategories.Cardio => expr,
        //     eMovementCategories.Flexibility => expr,
        //     eMovementCategories.Hybrid => expr,
        //     eMovementCategories.undefined => expr,
        //     _ => throw new ArgumentOutOfRangeException(nameof(category), category, null)
        // };
    }
}