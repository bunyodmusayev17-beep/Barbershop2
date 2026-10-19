namespace KnyazBarber.Api.Helpers;

public static class Policies
{
    /// <summary>Faqat admin.</summary>
    public const string Admin = "Admin";
    /// <summary>Admin yoki usta.</summary>
    public const string Staff = "Staff";
}

public static class AppClaims
{
    public const string BarberId = "barber_id";
}
