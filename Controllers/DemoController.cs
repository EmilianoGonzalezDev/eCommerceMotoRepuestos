using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using eCommerceMotoRepuestos.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.Options;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace eCommerceMotoRepuestos.Controllers;

[Route("demo")]
[EnableRateLimiting(RateLimitPolicies.DemoReset)]
public class DemoController(
    DemoResetService _demoResetService,
    AppSettingService _appSettingService,
    IOptions<DemoOptions> _demoOptions) : Controller
{
    private const string ResetTokenHeader = "X-Demo-Reset-Token";

    /// <summary>
    /// Scheduled reset (GitHub Actions). Authenticated by a shared secret header instead of a user session.
    /// </summary>
    [HttpPost("reset")]
    [AllowAnonymous]
    [IgnoreAntiforgeryToken]
    public async Task<IActionResult> Reset([FromHeader(Name = ResetTokenHeader)] string? token, CancellationToken cancellationToken)
    {
        var options = _demoOptions.Value;
        if (!options.Enabled || string.IsNullOrEmpty(options.ResetToken)) return NotFound();

        if (!TokensMatch(token, options.ResetToken)) return Unauthorized();

        var resetAtUtc = await _demoResetService.ResetAsync(cancellationToken);
        return Ok(new { resetAtUtc });
    }

    /// <summary>
    /// "Restablecer datos demo" button, available to the demo admins.
    /// </summary>
    [HttpPost("reset-now")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public async Task<IActionResult> ResetNow(CancellationToken cancellationToken)
    {
        if (!_demoOptions.Value.Enabled) return NotFound();

        await _demoResetService.ResetAsync(cancellationToken);

        // Demo accounts survive the reset, so keep them signed in with the new epoch; anyone else no longer exists.
        if (_demoOptions.Value.IsDemoAccount(User.FindFirst(ClaimTypes.Email)?.Value))
        {
            var epochClaim = await DemoSession.CreateEpochClaimAsync(_appSettingService);
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                DemoSession.WithEpoch(User, epochClaim),
                new AuthenticationProperties { AllowRefresh = true });
        }
        else
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        }

        HttpContext.Session.Clear();
        TempData["SuccessMessage"] = "Los datos de la demo se restablecieron a su estado inicial.";
        return RedirectToAction("Index", "Home");
    }

    private static bool TokensMatch(string? provided, string expected)
    {
        if (string.IsNullOrEmpty(provided)) return false;

        return CryptographicOperations.FixedTimeEquals(
            SHA256.HashData(Encoding.UTF8.GetBytes(provided)),
            SHA256.HashData(Encoding.UTF8.GetBytes(expected)));
    }
}
