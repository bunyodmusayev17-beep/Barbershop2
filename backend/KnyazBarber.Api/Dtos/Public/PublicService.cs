using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Public;

public record PublicService(int Id, LocalizedText Name, LocalizedText Description, decimal Price, int DurationMinutes, List<int> BarberIds);
