namespace GainsLab.Domain.Comparison;

public static class StringComparison
{
    public static bool Same(string? a, string? b)
    {
        var formatedA = StringFormater.Normalize(a);
        var formatedB = StringFormater.Normalize(b);

        return string.Equals(formatedA, formatedB, System.StringComparison.InvariantCultureIgnoreCase);
    }
}