namespace KnyazBarber.Api.Helpers;

public static class RateLimits
{
    /// <summary>Parolni terib topishga qarshi: bitta IP'dan daqiqasiga 10 urinish.</summary>
    public const string Login = "login";

    /// <summary>Spamga qarshi: bitta IP'dan daqiqasiga 5 ta zapis.</summary>
    public const string Booking = "booking";
}
