using KnyazBarber.Api.Entities;

namespace KnyazBarber.Api.Services.Telegram;

public interface ITelegramNotificationService
{
    /// <summary>Yangi yoki bekor qilingan navbat haqida xodimlarga xabar.</summary>
    Task HandleEventAsync(BotEvent e, CancellationToken ct);

    /// <summary>Navbatga 2 soat qolganda ustaga eslatma.</summary>
    Task SendRemindersAsync(CancellationToken ct);

    /// <summary>Kechki hisobot — kunning eng kech ish tugash vaqtidan 30 daqiqa keyin, kuniga bir marta.</summary>
    Task SendDailyReportIfDueAsync(CancellationToken ct);

    Task<Appointment?> LoadAsync(int id, CancellationToken ct);

    AppointmentCard ToCard(Appointment a);
}
