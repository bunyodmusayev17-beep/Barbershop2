using System.Security.Claims;
using Microsoft.IdentityModel.JsonWebTokens;
using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Helpers;

public static class ClaimsPrincipalExtensions
{
    public static int? UserId(this ClaimsPrincipal p) =>
        int.TryParse(p.FindFirstValue(JwtRegisteredClaimNames.Sub), out var id) ? id : null;

    public static int? BarberId(this ClaimsPrincipal p) =>
        int.TryParse(p.FindFirstValue(AppClaims.BarberId), out var id) ? id : null;

    public static bool IsAdmin(this ClaimsPrincipal p) => p.IsInRole(nameof(UserRole.Admin));

    /// <summary>Admin uchun null (hammasini ko'radi), usta uchun uning BarberId'si (ustasiz akkaunt — -1, hech narsa ko'rmaydi).</summary>
    public static int? OwnBarberId(this ClaimsPrincipal p) => p.IsAdmin() ? null : (p.BarberId() ?? -1);
}
