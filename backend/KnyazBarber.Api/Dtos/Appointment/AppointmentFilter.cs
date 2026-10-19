using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Dtos.Appointment;

/// <summary>Navbatlar ro'yxati filtri. Sana — mahalliy kun; bo'sh bo'lsa bugundan 30 kun.</summary>
public record AppointmentFilter(DateOnly? From, DateOnly? To, int? BarberId, AppointmentStatus? Status, string? Search);
