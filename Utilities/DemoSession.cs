using eCommerceMotoRepuestos.Models;
using eCommerceMotoRepuestos.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Security.Claims;

namespace eCommerceMotoRepuestos.Utilities;

/// <summary>
/// Ties auth cookies to the last demo reset: after a reset, users created during the demo no longer
/// exist, so cookies issued before it are rejected and the visitor is signed out.
/// </summary>
public static class DemoSession
{
    public const string EpochClaimType = "demo_epoch";

    public static string ToEpoch(DateTime? lastDemoReset)
    {
        return lastDemoReset?.Ticks.ToString(CultureInfo.InvariantCulture) ?? string.Empty;
    }

    public static async Task<Claim> CreateEpochClaimAsync(AppSettingService appSettingService)
    {
        return new Claim(EpochClaimType, ToEpoch(await appSettingService.GetLastDemoResetAsync()));
    }

    public static ClaimsPrincipal WithEpoch(ClaimsPrincipal principal, Claim epochClaim)
    {
        var claims = principal.Claims
            .Where(x => x.Type != EpochClaimType)
            .Append(epochClaim);

        return new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme));
    }

    public static async Task ValidatePrincipalAsync(CookieValidatePrincipalContext context)
    {
        var services = context.HttpContext.RequestServices;
        if (!services.GetRequiredService<IOptions<DemoOptions>>().Value.Enabled) return;

        var lastDemoReset = await services.GetRequiredService<AppSettingService>().GetLastDemoResetAsync();
        if (context.Principal?.FindFirst(EpochClaimType)?.Value == ToEpoch(lastDemoReset)) return;

        context.RejectPrincipal();
        await context.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
    }
}
