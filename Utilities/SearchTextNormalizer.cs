using eCommerceMotoRepuestos.Entities;
using System.Globalization;
using System.Text;

namespace eCommerceMotoRepuestos.Utilities;

public static class SearchTextNormalizer
{
    /// <summary>
    /// Lowercases, trims and removes diacritics so searches ignore case and accents.
    /// </summary>
    public static string Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value)) return string.Empty;

        var normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);
        var sb = new StringBuilder(normalized.Length);

        foreach (var c in normalized)
        {
            if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
            {
                sb.Append(c);
            }
        }

        return sb.ToString().Normalize(NormalizationForm.FormC);
    }

    /// <summary>
    /// Builds the value stored in <see cref="Product.SearchText"/>.
    /// </summary>
    public static string ForProduct(string? name, string? description)
    {
        return $"{Normalize(name)} {Normalize(description)}".Trim();
    }

    public static string[] SplitTerms(string? search)
    {
        return Normalize(search)
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
    }
}
