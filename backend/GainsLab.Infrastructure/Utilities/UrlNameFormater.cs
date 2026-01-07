using System.Globalization;
using System.Text;

namespace GainsLab.Infrastructure.Utilities;

public static class UrlNameFormater
{
    public static string Format(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return "unknown";
        }

        var normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(normalized.Length);
        var lastWasSeparator = false;

        foreach (var c in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(c);
            if (category == UnicodeCategory.NonSpacingMark)
            {
                continue; // strip diacritics
            }

            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
                lastWasSeparator = false;
                continue;
            }

            if (char.IsWhiteSpace(c) || c == '-' || c == '_')
            {
                if (!lastWasSeparator && builder.Length > 0)
                {
                    builder.Append('-');
                    lastWasSeparator = true;
                }

                continue;
            }

            // Ignore any other character (symbols, punctuation, etc.)
        }

        var formatted = builder.ToString().Trim('-');
        return string.IsNullOrEmpty(formatted) ? "unknown" : formatted;
    }
}
