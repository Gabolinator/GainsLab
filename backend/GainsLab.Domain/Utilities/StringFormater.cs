namespace GainsLab.Domain;

public static class StringFormater
{
    public static char[] DefaultRemovedChar => [' ','_','/','\n','|','$','@','&'];

    public static string Trim(string text, params char[] args)
    {
        if(string.IsNullOrEmpty(text)) return string.Empty;
        var trimChars = DefaultRemovedChar.Concat(args).ToArray();

        return text.Trim(trimChars);
    }

    public static string RemoveUnwantedChar(string? text, params char[] args)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
        
        var removedChars = DefaultRemovedChar.Concat(args).ToHashSet();
        
        return new string(
            text.Where(c => !removedChars.Contains(c))
                .ToArray()
        );
    }

    public static string Normalize(string? text, params char[] args)
    {
        var t = RemoveUnwantedChar(text, args);
        return t.ToLowerInvariant().Trim(args);
    }
}