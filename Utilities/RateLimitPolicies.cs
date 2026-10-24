using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;

namespace eCommerceMotoRepuestos.Utilities;

/// <summary>
/// Per-IP request limits. The site is a public demo with shared admin credentials,
/// so brute force, spam sign-ups and bulk writes have to be throttled.
/// </summary>
public static class RateLimitPolicies
{
    public const string Auth = "auth";
    public const string Writes = "writes";
    public const string DemoReset = "demo-reset";

    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services)
    {
        return services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(context =>
                FixedWindowPerIp(context, permitLimit: 300));

            options.AddPolicy(Auth, context => FixedWindowPerIp(context, permitLimit: 10));
            options.AddPolicy(Writes, context => FixedWindowPerIp(context, permitLimit: 30));
            options.AddPolicy(DemoReset, context => FixedWindowPerIp(context, permitLimit: 2));
        });
    }

    private static RateLimitPartition<string> FixedWindowPerIp(HttpContext context, int permitLimit)
    {
        var clientIp = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

        return RateLimitPartition.GetFixedWindowLimiter(clientIp, _ => new FixedWindowRateLimiterOptions
        {
            PermitLimit = permitLimit,
            Window = TimeSpan.FromMinutes(1),
            QueueLimit = 0
        });
    }
}
