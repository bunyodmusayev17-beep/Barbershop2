using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Configurations;

public static class JwtConfigurations
{
    /// <summary>JWT autentifikatsiya va rollar bo'yicha ruxsatlar (Admin, Staff = admin yoki usta).</summary>
    public static void AddJwtConfigurations(this WebApplicationBuilder builder)
    {
        var jwt = builder.Configuration.GetSection(JwtOptions.Section).Get<JwtOptions>() ?? new JwtOptions();
        if (Encoding.UTF8.GetByteCount(jwt.Key) < 32)
            throw new InvalidOperationException("Jwt:Key sozlanmagan yoki juda qisqa (kamida 32 bayt). Dev uchun: scripts/dev-secrets.ps1");

        builder.Services.Configure<JwtOptions>(builder.Configuration.GetSection(JwtOptions.Section));
        builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(o =>
            {
                o.MapInboundClaims = false;
                o.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidIssuer = jwt.Issuer,
                    ValidAudience = jwt.Audience,
                    IssuerSigningKey = TokenService.SigningKey(jwt),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role,
                    ClockSkew = TimeSpan.FromMinutes(1),
                };
            });
        builder.Services.AddAuthorizationBuilder()
            .AddPolicy(Policies.Admin, p => p.RequireRole(nameof(UserRole.Admin)))
            .AddPolicy(Policies.Staff, p => p.RequireRole(nameof(UserRole.Admin), nameof(UserRole.Barber)));
    }
}
