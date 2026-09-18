using ABC_construction.Configuration;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace ABC_construction.Controllers;

/// <summary>Target of the header language switcher.</summary>
public class LanguageController : Controller
{
    private readonly SiteLanguageOptions _languages;

    public LanguageController(IOptions<SiteLanguageOptions> languages)
    {
        _languages = languages.Value;
    }

    /// <summary>
    /// Remembers the chosen language in a cookie and returns the visitor to the
    /// page they were on. A plain link rather than a form so it works without
    /// JavaScript and needs no anti-forgery token on every public page.
    /// </summary>
    [HttpGet("/language/{culture}")]
    [ResponseCache(NoStore = true, Location = ResponseCacheLocation.None)]
    public IActionResult Set(string culture, [FromQuery] string? returnUrl)
    {
        var supported = _languages.EffectiveCultures
            .FirstOrDefault(c => string.Equals(c, culture, StringComparison.OrdinalIgnoreCase));

        if (supported is not null)
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(supported)),
                new CookieOptions
                {
                    Expires = DateTimeOffset.UtcNow.AddYears(1),
                    // A preference the visitor asked for, not tracking.
                    IsEssential = true,
                    HttpOnly = true,
                    Secure = Request.IsHttps,
                    SameSite = SameSiteMode.Lax
                });
        }

        // Local only, so the switcher cannot be used as an open redirect.
        return LocalRedirect(Url.IsLocalUrl(returnUrl) ? returnUrl! : "/");
    }
}
