using System.Net;
using System.Text;
using KnyazBarber.Api.Dtos.Public;
using Telegram.Bot.Types.ReplyMarkups;

namespace KnyazBarber.Api.Services.Telegram;

/// <summary>
/// Mijoz uchun bot: saytdagi ma'lumot (manzil, ish vaqti, reyting, telefon) va tugmalar —
/// navbat olish va narxlar saytning o'zini (Mini App) ochadi. Sof funksiyalar, testlanadi.
/// </summary>
public static class TelegramWelcome
{
    // Callback: "i:{amal}" — xodim bo'lishi shart emas
    public const string Map = "i:map", Call = "i:call";

    private static readonly Dictionary<string, string[]> Days = new()
    {
        ["uz"] = ["Ya", "Du", "Se", "Ch", "Pa", "Ju", "Sh"],
        ["ru"] = ["Вс", "Пн", "Вт", "Ср", "Чт", "Пт", "Сб"],
        ["en"] = ["Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat"],
    };

    /// <summary>Telegram'dagi til kodi bo'yicha: ru / en, qolgan hammasi — o'zbekcha.</summary>
    public static string Lang(string? code) => code switch
    {
        not null when code.StartsWith("ru") => "ru",
        not null when code.StartsWith("en") => "en",
        _ => "uz",
    };

    public static string Text(PublicSettings s, string lang)
    {
        var sb = new StringBuilder();
        sb.Append("💈 <b>").Append(H(s.BusinessName)).Append("</b>\n");
        if (s.RatingCount > 0)
            sb.Append("⭐ ").Append(s.Rating.ToString("0.0", System.Globalization.CultureInfo.InvariantCulture)).Append(" · ")
                .Append(s.RatingCount).Append(L(lang, " ta baho", " оценок", " reviews")).Append('\n');
        // Tarjima kiritilmagan bo'lsa — o'zbekchasi
        var address = L(lang, s.Address.Uz, s.Address.Ru, s.Address.En) is {Length: > 0} a ? a : s.Address.Uz;
        if (!string.IsNullOrWhiteSpace(address)) sb.Append("📍 ").Append(H(address)).Append('\n');
        var hours = Hours(s.Hours, lang);
        if (hours is not null) sb.Append("🕐 ").Append(hours).Append('\n');
        if (!string.IsNullOrWhiteSpace(s.Phone)) sb.Append("📞 ").Append(TelegramMessages.FormatPhone(s.Phone)).Append('\n');
        sb.Append('\n').Append(L(lang,
            "Onlayn navbat oling — pastdagi tugmani bosing 👇",
            "Запишитесь онлайн — нажмите кнопку ниже 👇",
            "Book online — tap the button below 👇"));
        return sb.ToString();
    }

    /// <summary>Hamma kun bir xil bo'lsa — bitta qator ("Har kuni 11:00–23:00"), aks holda kunma-kun (dushanbadan).</summary>
    public static string? Hours(IReadOnlyList<PublicHours> hours, string lang)
    {
        if (hours.Count == 0 || hours.All(h => h.Open is null)) return null;
        if (hours.Count == 7 && hours.All(h => h.Open == hours[0].Open && h.Close == hours[0].Close))
            return $"{L(lang, "Har kuni", "Ежедневно", "Every day")} {hours[0].Open}–{hours[0].Close}";

        var names = Days[lang];
        var closed = L(lang, "dam olish", "выходной", "closed");
        return string.Join(", ", hours.OrderBy(h => ((int)h.Day + 6) % 7)
            .Select(h => $"{names[(int)h.Day]} {(h.Open is null ? closed : $"{h.Open}–{h.Close}")}"));
    }

    /// <summary>Mini App manzili bo'lmasa (HTTPS yo'q) — faqat manzil va qo'ng'iroq tugmalari.</summary>
    public static InlineKeyboardMarkup Keyboard(string? miniAppUrl, string lang)
    {
        var rows = new List<InlineKeyboardButton[]>();
        if (!string.IsNullOrWhiteSpace(miniAppUrl))
        {
            var url = miniAppUrl.TrimEnd('/');
            rows.Add([InlineKeyboardButton.WithWebApp(L(lang, "✂️ Navbat olish", "✂️ Записаться", "✂️ Book now"), new($"{url}/book"))]);
            rows.Add([InlineKeyboardButton.WithWebApp(L(lang, "💈 Xizmatlar va narxlar", "💈 Услуги и цены", "💈 Services & prices"), new($"{url}/#services"))]);
        }
        rows.Add([
            InlineKeyboardButton.WithCallbackData(L(lang, "📍 Manzil", "📍 Адрес", "📍 Location"), Map),
            InlineKeyboardButton.WithCallbackData(L(lang, "📞 Qo'ng'iroq", "📞 Позвонить", "📞 Call us"), Call),
        ]);
        return new InlineKeyboardMarkup(rows);
    }

    private static string L(string lang, string uz, string ru, string en) => lang switch {"ru" => ru, "en" => en, _ => uz};

    private static string H(string s) => WebUtility.HtmlEncode(s);
}
