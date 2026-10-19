using KnyazBarber.Api.Dtos.Settings;

namespace KnyazBarber.Api.Services;

public interface ISettingsService
{
    Task<SettingsDto> GetAsync(CancellationToken ct);
    Task UpdateAsync(SettingsDto input, CancellationToken ct);
}
