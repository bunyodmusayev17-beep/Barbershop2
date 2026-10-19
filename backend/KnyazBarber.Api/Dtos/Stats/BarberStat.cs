namespace KnyazBarber.Api.Dtos.Stats;

public record BarberStat(int BarberId, string Name, int TodayCount, int WeekCount, decimal WeekRevenue);
