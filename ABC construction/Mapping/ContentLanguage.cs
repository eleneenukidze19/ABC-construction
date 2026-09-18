using System.Globalization;

namespace ABC_construction.Mapping;

/// <summary>
/// Chooses between the English and Georgian versions of admin-entered content.
/// <para>
/// Both versions travel together through the DTOs (and so the cache); the pick
/// happens only when a view model is built, for the current request's language.
/// A Georgian field left empty falls back to English, so a partly translated
/// record never shows a blank on the Georgian site.
/// </para>
/// </summary>
public static class ContentLanguage
{
    public static bool IsGeorgian =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "ka";

    public static string Pick(string english, string? georgian) =>
        IsGeorgian && !string.IsNullOrWhiteSpace(georgian) ? georgian : english;

    public static string? PickOptional(string? english, string? georgian) =>
        IsGeorgian && !string.IsNullOrWhiteSpace(georgian) ? georgian : english;
}
