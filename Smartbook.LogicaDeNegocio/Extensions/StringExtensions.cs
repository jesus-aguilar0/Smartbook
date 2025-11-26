using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Smartbook.LogicaDeNegocio.Extensions;

public static class StringExtensions
{
    public static string RemoveAccents(this string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return text;

        var normalizedString = text.Normalize(NormalizationForm.FormD);
        var stringBuilder = new StringBuilder();

        foreach (var c in normalizedString)
        {
            var unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
            if (unicodeCategory != UnicodeCategory.NonSpacingMark)
            {
                stringBuilder.Append(c);
            }
        }

        return stringBuilder.ToString().Normalize(NormalizationForm.FormC);
    }

    public static string Sanitize(this string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return string.Empty;

        // Remove HTML tags
        input = Regex.Replace(input, "<.*?>", string.Empty);
        
        // Remove script tags and their content
        input = Regex.Replace(input, @"<script\b[^<]*(?:(?!<\/script>)<[^<]*)*<\/script>", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove event handlers
        input = Regex.Replace(input, @"on\w+\s*=\s*[""'][^""']*[""']", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove SQL injection patterns
        input = Regex.Replace(input, @"(\b(SELECT|INSERT|UPDATE|DELETE|DROP|CREATE|ALTER|EXEC|EXECUTE)\b)", string.Empty, RegexOptions.IgnoreCase);
        
        // Remove special characters that could be used for injection
        input = input.Replace("'", "''");
        input = input.Replace(";", "");
        input = input.Replace("--", "");
        input = input.Replace("/*", "");
        input = input.Replace("*/", "");
        
        return input.Trim();
    }
}

