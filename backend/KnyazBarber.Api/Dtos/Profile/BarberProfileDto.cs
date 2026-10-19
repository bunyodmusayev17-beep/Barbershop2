namespace KnyazBarber.Api.Dtos.Profile;

/// <summary>Usta kabinetidagi "Profilim": o'z rasmi va ishlari. MaxWorks — nechtagacha ish yuklash mumkin.</summary>
public record BarberProfileDto(int BarberId, string Name, string? PhotoUrl, List<BarberWorkDto> Works, int MaxWorks);
