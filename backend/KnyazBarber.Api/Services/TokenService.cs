using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using KnyazBarber.Api.Configurations;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;

namespace KnyazBarber.Api.Services;

public class TokenService(IOptions<JwtOptions> options, TimeProvider time) : ITokenService
{
    private readonly JwtOptions _opt = options.Value;
    private readonly JsonWebTokenHandler _handler = new();

    public (string Token, DateTimeOffset ExpiresAt) Create(User user)
    {
        var now = time.GetUtcNow();
        var expires = now.AddMinutes(_opt.ExpiresMinutes);
        var claims = new Dictionary<string, object>
        {
            [JwtRegisteredClaimNames.Sub] = user.Id.ToString(),
            [ClaimTypes.Name] = user.Login,
            [ClaimTypes.Role] = user.Role.ToString(),
        };
        if (user.BarberId is { } barberId)
            claims[AppClaims.BarberId] = barberId.ToString();

        var token = _handler.CreateToken(new SecurityTokenDescriptor
        {
            Issuer = _opt.Issuer,
            Audience = _opt.Audience,
            Claims = claims,
            NotBefore = now.UtcDateTime,
            IssuedAt = now.UtcDateTime,
            Expires = expires.UtcDateTime,
            SigningCredentials = new SigningCredentials(SigningKey(_opt), SecurityAlgorithms.HmacSha256),
        });
        return (token, expires);
    }

    public static SymmetricSecurityKey SigningKey(JwtOptions opt) => new(Encoding.UTF8.GetBytes(opt.Key));
}
