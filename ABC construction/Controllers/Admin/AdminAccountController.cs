using ABC_construction.Models;
using ABC_construction.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace ABC_construction.Controllers.Admin;

/// <summary>
/// Admin sign-in and sign-out (README section 5). Paths match the cookie
/// settings configured in Program.cs.
/// </summary>
[Route("admin")]
public class AdminAccountController : Controller
{
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ILogger<AdminAccountController> _logger;

    public AdminAccountController(
        SignInManager<ApplicationUser> signInManager,
        UserManager<ApplicationUser> userManager,
        ILogger<AdminAccountController> logger)
    {
        _signInManager = signInManager;
        _userManager = userManager;
        _logger = logger;
    }

    [HttpGet("login")]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        if (_signInManager.IsSignedIn(User))
        {
            return RedirectToAction("Dashboard", "Admin");
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _userManager.FindByEmailAsync(model.Email);

        // The password is verified before the cookie is issued, so an account
        // without panel access never gets an authenticated session it would
        // only be denied on the next request.
        // Qualified: Microsoft.AspNetCore.Mvc also defines a SignInResult.
        var result = user is null
            ? Microsoft.AspNetCore.Identity.SignInResult.Failed
            : await _signInManager.CheckPasswordSignInAsync(user, model.Password, lockoutOnFailure: true);

        if (result.IsLockedOut)
        {
            _logger.LogWarning("Sign-in blocked: {Email} is locked out.", model.Email);
            ModelState.AddModelError(string.Empty,
                "This account is temporarily locked after too many failed attempts. Try again in 15 minutes.");
            return View(model);
        }

        if (!result.Succeeded)
        {
            _logger.LogWarning("Failed sign-in attempt for {Email}.", model.Email);

            // Deliberately does not reveal whether the account exists.
            ModelState.AddModelError(string.Empty, "Incorrect email address or password.");

            return View(model);
        }

        // Credentials are correct — authorisation is a separate question.
        // Without this, a roleless account would sign in successfully and then
        // be bounced to /admin/denied, which reads like a broken login.
        var roles = await _userManager.GetRolesAsync(user!);

        if (!ApplicationRoles.GrantsPanelAccess(roles))
        {
            _logger.LogWarning(
                "Sign-in refused for {Email}: no role granting admin panel access.", model.Email);

            ModelState.AddModelError(string.Empty,
                "This account does not have access to the administration panel. Ask an administrator to grant you a role.");

            return View(model);
        }

        await _signInManager.SignInAsync(user!, model.RememberMe);

        _logger.LogInformation("Admin {Email} signed in.", model.Email);

        return RedirectToLocal(model.ReturnUrl);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        var user = User.Identity?.Name;
        await _signInManager.SignOutAsync();

        _logger.LogInformation("Admin {Email} signed out.", user);

        return RedirectToAction(nameof(Login));
    }

    [HttpGet("denied")]
    public IActionResult Denied() => View();

    /// <summary>
    /// Only redirects to same-site paths — an attacker-supplied returnUrl must
    /// never bounce a freshly authenticated admin to an external host.
    /// </summary>
    private IActionResult RedirectToLocal(string? returnUrl)
    {
        if (!string.IsNullOrWhiteSpace(returnUrl) && Url.IsLocalUrl(returnUrl))
        {
            return Redirect(returnUrl);
        }

        return RedirectToAction("Dashboard", "Admin");
    }
}
