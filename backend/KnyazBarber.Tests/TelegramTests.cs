using System.Text;
using Microsoft.AspNetCore.WebUtilities;
using KnyazBarber.Api.Dtos.Public;
using KnyazBarber.Api.Entities;
using KnyazBarber.Api.Services.Telegram;

namespace KnyazBarber.Tests;

public class TelegramInitDataTests
{
    private const string Token = "123456:TEST-token";
    private static readonly DateTimeOffset Now = new(2026, 10, 13, 10, 0, 0, TimeSpan.Zero);

    /// <summary>Telegram qanday imzolasa, shunday imzolangan initData yasaydi.</summary>
    private static string Build(Dictionary<string, string> fields, string token = Token)
    {
        var check = string.Join('\n', fields.OrderBy(f => f.Key, StringComparer.Ordinal).Select(f => $"{f.Key}={f.Value}"));
        var hash = Convert.ToHexStringLower(TelegramInitData.Sign(check, token));
        return QueryHelpers.AddQueryString("", fields.Append(new("hash", hash))!).TrimStart('?');
    }

    private static Dictionary<string, string> Fields(DateTimeOffset authDate) => new()
    {
        ["auth_date"] = authDate.ToUnixTimeSeconds().ToString(),
        ["query_id"] = "AAH",
        ["user"] = """{"id":42,"first_name":"Sardor","username":"Sardor_Barber"}""",
    };

    [Fact]
    public void Valid_signature_returns_user()
    {
        var user = TelegramInitData.Validate(Build(Fields(Now.AddMinutes(-1))), Token, Now);
        Assert.NotNull(user);
        Assert.Equal(42, user.Id);
        Assert.Equal("Sardor_Barber", user.Username);
    }

    [Fact]
    public void Tampered_user_is_rejected()
    {
        var data = Build(Fields(Now)).Replace("%22id%22%3A42", "%22id%22%3A43");
        Assert.Null(TelegramInitData.Validate(data, Token, Now));
    }

    [Fact]
    public void Other_bot_token_is_rejected() =>
        Assert.Null(TelegramInitData.Validate(Build(Fields(Now), "999:other"), Token, Now));

    [Fact]
    public void Old_init_data_is_rejected() =>
        Assert.Null(TelegramInitData.Validate(Build(Fields(Now.AddDays(-2))), Token, Now));

    [Theory]
    [InlineData("")]
    [InlineData("user=1")]
    [InlineData("hash=zz&user=1")]
    public void Garbage_is_rejected(string data) => Assert.Null(TelegramInitData.Validate(data, Token, Now));
}

public class TelegramMessagesTests
{
    private static AppointmentCard Card(AppointmentStatus status = AppointmentStatus.Confirmed, string? comment = null) => new(
        7, "Aziz <script>", "+998901234567", "Soch olish", "Sardor",
        new DateTime(2026, 10, 13, 14, 0, 0), new DateTime(2026, 10, 13, 14, 45, 0), 80_000, comment, status);

    [Fact]
    public void Card_contains_details_and_escapes_html()
    {
        var text = TelegramMessages.Card(Card(comment: "Tez"), CardKind.New);
        Assert.Contains("Yangi navbat", text);
        Assert.Contains("Aziz &lt;script&gt;", text);
        Assert.Contains("+998 90 123 45 67", text);
        Assert.Contains("13.10 (Seshanba)", text);
        Assert.Contains("14:00–14:45", text);
        Assert.Contains("80 000 so'm", text);
        Assert.Contains("Tez", text);
    }

    [Fact]
    public void Final_card_shows_status_and_footer()
    {
        var text = TelegramMessages.Card(Card(AppointmentStatus.Completed), CardKind.Updated, "sardor, 14:50");
        Assert.Contains("✅ Keldi", text);
        Assert.Contains("sardor, 14:50", text);
    }

    [Fact]
    public void Active_appointment_has_four_buttons_final_has_only_call()
    {
        Assert.Equal(4, TelegramMessages.Keyboard(7, AppointmentStatus.Confirmed).InlineKeyboard.SelectMany(r => r).Count());
        var final = TelegramMessages.Keyboard(7, AppointmentStatus.Cancelled).InlineKeyboard.SelectMany(r => r).ToList();
        Assert.Single(final);
        Assert.Equal("a:7:call", final[0].CallbackData);
    }

    [Theory]
    [InlineData("a:15:done", true, 15, "done")]
    [InlineData("a:3:cancel_yes", true, 3, "cancel_yes")]
    [InlineData("b:3:done", false, 0, "")]
    [InlineData("a:x:done", false, 0, "")]
    [InlineData(null, false, 0, "")]
    public void Callback_data_is_parsed(string? data, bool ok, int id, string action)
    {
        Assert.Equal(ok, TelegramMessages.TryParseCallback(data, out var parsedId, out var parsedAction));
        Assert.Equal(id, parsedId);
        Assert.Equal(action, parsedAction);
    }

    [Fact]
    public void All_callback_data_fits_telegram_limit() =>
        Assert.True(Encoding.UTF8.GetByteCount(TelegramMessages.CallbackData(int.MaxValue, TelegramMessages.CancelYes)) <= 64);

    [Fact]
    public void Report_time_is_latest_end_plus_30_minutes()
    {
        var date = new DateOnly(2026, 10, 13);
        var at = TelegramMessages.ReportTime(date, [(new TimeOnly(21, 0), false), (new TimeOnly(23, 0), false), (new TimeOnly(23, 59), true)]);
        Assert.Equal(new DateTime(2026, 10, 13, 23, 30, 0), at);
    }

    [Fact]
    public void Report_time_crosses_midnight()
    {
        var at = TelegramMessages.ReportTime(new DateOnly(2026, 10, 13), [(new TimeOnly(23, 45), false)]);
        Assert.Equal(new DateTime(2026, 10, 14, 0, 15, 0), at);
    }

    [Fact]
    public void No_report_when_nobody_works() =>
        Assert.Null(TelegramMessages.ReportTime(new DateOnly(2026, 10, 13), [(new TimeOnly(23, 0), true)]));

    [Fact]
    public void Daily_report_counts_and_revenue()
    {
        ReportRow[] rows =
        [
            new("Jasur", AppointmentStatus.Completed, 80_000),
            new("Jasur", AppointmentStatus.Completed, 120_000),
            new("Sardor", AppointmentStatus.Completed, 50_000),
            new("Sardor", AppointmentStatus.NoShow, 50_000),
            new("Sardor", AppointmentStatus.Cancelled, 80_000),
            new("Bekzod", AppointmentStatus.Confirmed, 60_000),
        ];
        var text = TelegramMessages.DailyReport(new DateOnly(2026, 10, 13), rows);
        Assert.Contains("Jami navbat: <b>6</b>", text);
        Assert.Contains("✅ Keldi: 3", text);
        Assert.Contains("🚫 Kelmadi: 1", text);
        Assert.Contains("❌ Bekor: 1", text);
        Assert.Contains("⏳ Belgilanmagan: 1", text);
        Assert.Contains("Tushum: <b>250 000 so'm</b>", text);
        Assert.Contains("Jasur — 2 ta, 200 000 so'm", text);
        Assert.True(text.IndexOf("Jasur", StringComparison.Ordinal) < text.IndexOf("Sardor —", StringComparison.Ordinal));
    }

    [Fact]
    public void Empty_day_report() =>
        Assert.Contains("navbat bo'lmadi", TelegramMessages.DailyReport(new DateOnly(2026, 10, 13), []));
}

public class TelegramWelcomeTests
{
    private static List<PublicHours> Every(string open, string close) =>
        Enum.GetValues<DayOfWeek>().Select(d => new PublicHours(d, open, close)).ToList();

    private static PublicSettings Settings(List<PublicHours> hours) => new(
        "Knyaz <Barber>", new LocalizedText("Sergeli-V, 2", "Сергели-V, 2", ""), "+998 90 965 18 77", null, null,
        41.2, 69.2, 5.0, 46, 14, hours);

    [Theory]
    [InlineData("ru", "ru")]
    [InlineData("ru-RU", "ru")]
    [InlineData("en", "en")]
    [InlineData("uz", "uz")]
    [InlineData("de", "uz")]
    [InlineData(null, "uz")]
    public void Language_falls_back_to_uzbek(string? code, string expected) => Assert.Equal(expected, TelegramWelcome.Lang(code));

    [Fact]
    public void Text_has_site_info_and_escapes_html()
    {
        var text = TelegramWelcome.Text(Settings(Every("11:00", "23:00")), "uz");
        Assert.Contains("Knyaz &lt;Barber&gt;", text);
        Assert.Contains("⭐ 5.0 · 46 ta baho", text);
        Assert.Contains("📍 Sergeli-V, 2", text);
        Assert.Contains("Har kuni 11:00–23:00", text);
        Assert.Contains("+998 90 965 18 77", text);
    }

    [Fact]
    public void Missing_translation_uses_uzbek_address() =>
        Assert.Contains("📍 Sergeli-V, 2", TelegramWelcome.Text(Settings(Every("10:00", "22:00")), "en"));

    [Fact]
    public void Different_days_are_listed_from_monday()
    {
        var hours = Every("11:00", "23:00");
        hours[(int)DayOfWeek.Sunday] = new PublicHours(DayOfWeek.Sunday, null, null);
        var line = TelegramWelcome.Hours(hours, "uz")!;
        Assert.StartsWith("Du 11:00–23:00", line);
        Assert.EndsWith("Ya dam olish", line);
    }

    [Fact]
    public void No_hours_when_nobody_works() =>
        Assert.Null(TelegramWelcome.Hours(Enum.GetValues<DayOfWeek>().Select(d => new PublicHours(d, null, null)).ToList(), "uz"));

    [Fact]
    public void Keyboard_opens_site_pages_when_mini_app_is_set()
    {
        var rows = TelegramWelcome.Keyboard("https://knyaz.example/", "uz").InlineKeyboard.Select(r => r.ToList()).ToList();
        Assert.Equal(3, rows.Count);
        Assert.Equal("https://knyaz.example/book", rows[0][0].WebApp!.Url);
        Assert.Equal("https://knyaz.example/#services", rows[1][0].WebApp!.Url);
        Assert.Equal([TelegramWelcome.Map, TelegramWelcome.Call], rows[2].Select(b => b.CallbackData));
    }

    [Fact]
    public void Keyboard_without_mini_app_has_only_info_buttons() =>
        Assert.Single(TelegramWelcome.Keyboard(null, "ru").InlineKeyboard);
}
