using KnyazBarber.Api.Dtos.Appointment;

namespace KnyazBarber.Api.Dtos.Stats;

public record StatsDto(
    int TodayCount, decimal TodayRevenue, int WeekCount, decimal WeekRevenue,
    int UpcomingCount, int NoShowWeek, List<DayStat> Last7Days, List<BarberStat> Barbers,
    List<AppointmentDto> NextUp);
