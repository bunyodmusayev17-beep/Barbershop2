namespace KnyazBarber.Api.Entities;

public class User
{
    public int Id { get; set; }
    public string Login { get; set; } = "";
    public string? Email { get; set; }
    public string PasswordHash { get; set; } = "";
    public UserRole Role { get; set; }
    public int? BarberId { get; set; }
    public Barber? Barber { get; set; }
    /// <summary>Admin kiritgan Telegram username (kichik harflarda, @ siz). Bot/Mini App orqali birinchi kirishda TelegramUserId bog'lanadi.</summary>
    public string? TelegramUsername { get; set; }
    /// <summary>Bog'langan Telegram akkaunt ID (shaxsiy chat ID ham shu).</summary>
    public long? TelegramUserId { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
}
