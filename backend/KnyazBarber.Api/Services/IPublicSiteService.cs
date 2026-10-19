using KnyazBarber.Api.Dtos.Public;

namespace KnyazBarber.Api.Services;

public interface IPublicSiteService
{
    /// <summary>Saytning ommaviy qismi uchun barcha kontent — bitta so'rovda.</summary>
    Task<SiteResponse> GetSiteAsync(CancellationToken ct);
}
