namespace KnyazBarber.Api.Configurations;

public static class CorsConfigurations
{
    public const string FrontendPolicy = "frontend";

    /// <summary>Ruxsat etilgan manzillar — Cors:Origins (prod'da frontend o'sha domendan, CORS kerak emas).</summary>
    public static void AddCorsConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.AddCors(options => options.AddPolicy(FrontendPolicy, policy =>
            policy.WithOrigins(builder.Configuration.GetSection("Cors:Origins").Get<string[]>() ?? [])
                .AllowAnyHeader()
                .AllowAnyMethod()));
    }
}
