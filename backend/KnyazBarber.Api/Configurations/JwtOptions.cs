namespace KnyazBarber.Api.Configurations;

public class JwtOptions
{
    public const string Section = "Jwt";

    public string Issuer { get; set; } = "knyazbarber";
    public string Audience { get; set; } = "knyazbarber";
    /// <summary>Kamida 32 bayt (base64). Dev: user-secrets, prod: muhit o'zgaruvchisi.</summary>
    public string Key { get; set; } = "";
    public int ExpiresMinutes { get; set; } = 720;
}
