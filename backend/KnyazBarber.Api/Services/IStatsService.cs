using KnyazBarber.Api.Dtos.Stats;

namespace KnyazBarber.Api.Services;

public interface IStatsService
{
    /// <summary>Dashboard: bugun, oxirgi 7 kun, ustalar bo'yicha va yaqin navbatlar.</summary>
    Task<StatsDto> GetAsync(CancellationToken ct);
}
