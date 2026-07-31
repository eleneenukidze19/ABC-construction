using Microsoft.AspNetCore.Identity;

namespace ABC_construction.Models;

/// <summary>
/// Admin panel account. Extends the ASP.NET Core Identity user so extra profile
/// fields can be added later without another migration of the Identity schema.
/// </summary>
public class ApplicationUser : IdentityUser
{
    [System.ComponentModel.DataAnnotations.MaxLength(150)]
    public string? DisplayName { get; set; }

    public DateTime CreatedDate { get; set; }
}
