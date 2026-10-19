using System.Threading.RateLimiting;
using KnyazBarber.Api.Helpers;

namespace KnyazBarber.Api.Configurations;

public static class RateLimitConfigurations
{
    public static void AddRateLimitConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.AddRateLimiter(o =>
        {
            o.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
            o.AddPolicy(RateLimits.Login, ctx => RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions {PermitLimit = 10, Window = TimeSpan.FromMinutes(1)}));
            o.AddPolicy(RateLimits.Booking, ctx => RateLimitPartition.GetFixedWindowLimiter(
                ctx.Connection.RemoteIpAddress?.ToString() ?? "unknown",
                _ => new FixedWindowRateLimiterOptions {PermitLimit = 5, Window = TimeSpan.FromMinutes(1)}));
        });
    }
}
