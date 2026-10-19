using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Services;

public interface ITokenService
{
    (string Token, DateTimeOffset ExpiresAt) Create(User user);
}
