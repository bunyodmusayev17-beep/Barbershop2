using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Public;

public record PublicImage(int Id, string Url, LocalizedText Caption, int? BarberId);
