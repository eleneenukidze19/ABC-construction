using Microsoft.Extensions.Localization;

// The assembly is named "ABC construction" but its resources are embedded under
// the ABC_construction root namespace. Without this the localizer would look
// for "ABC construction.Resources.SharedResource" and never find a translation.
[assembly: RootNamespace("ABC_construction")]

namespace ABC_construction;

/// <summary>
/// Marker type for the public site's interface text. Translations live in
/// Resources/SharedResource.ka.resx, keyed by the English text, so English
/// needs no resource file: a missing key simply renders as itself.
/// </summary>
public class SharedResource
{
}
