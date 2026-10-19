namespace KnyazBarber.Api.Dtos.Public;

/// <summary>Saytning ommaviy qismi uchun barcha kontent — bitta so'rovda.</summary>
public record SiteResponse(PublicSettings Settings, List<PublicService> Services, List<PublicBarber> Barbers, List<PublicReview> Reviews, List<PublicImage> Gallery);
