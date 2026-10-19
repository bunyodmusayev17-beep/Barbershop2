using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Dtos.Settings;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;

namespace KnyazBarber.Api.Services;

public class SettingsService(IBaseRepository<SiteSettings> settings) : ISettingsService
{
    public async Task<SettingsDto> GetAsync(CancellationToken ct)
    {
        var s = await settings.QueryNoTracking().OrderBy(x => x.Id).FirstAsync(ct);
        return new SettingsDto(s.BusinessName, s.Address, s.Phone, s.Telegram, s.Instagram, s.Latitude, s.Longitude,
            s.Rating, s.RatingCount, s.SlotStepMinutes, s.MinLeadMinutes, s.BookingHorizonDays);
    }

    public async Task UpdateAsync(SettingsDto input, CancellationToken ct)
    {
        var s = await settings.Query().OrderBy(x => x.Id).FirstAsync(ct);
        s.BusinessName = input.BusinessName.Trim();
        s.Address = input.Address.Clean(200);
        s.Phone = input.Phone.Trim();
        s.Telegram = CleanHandle(input.Telegram);
        s.Instagram = CleanHandle(input.Instagram);
        s.Latitude = input.Latitude;
        s.Longitude = input.Longitude;
        s.Rating = Math.Round(input.Rating, 1);
        s.RatingCount = input.RatingCount;
        s.SlotStepMinutes = input.SlotStepMinutes;
        s.MinLeadMinutes = input.MinLeadMinutes;
        s.BookingHorizonDays = input.BookingHorizonDays;
        await settings.SaveChangesAsync(ct);
    }

    /// <summary>"@knyazbarber", "https://t.me/knyazbarber" → "knyazbarber"</summary>
    private static string? CleanHandle(string? v)
    {
        if (string.IsNullOrWhiteSpace(v)) return null;
        var s = v.Trim().TrimEnd('/');
        var slash = s.LastIndexOf('/');
        if (slash >= 0) s = s[(slash + 1)..];
        return s.TrimStart('@');
    }
}
