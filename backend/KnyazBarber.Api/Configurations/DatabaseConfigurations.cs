using Microsoft.EntityFrameworkCore;
using KnyazBarber.Api.Data;

namespace KnyazBarber.Api.Configurations;

public static class DatabaseConfigurations
{
    public static void AddDatabaseConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(builder.Configuration.GetConnectionString("Default"),
                npgsql => npgsql.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery)));
    }

    /// <summary>Migratsiyalar va boshlang'ich ma'lumotlar (prod'da faqat sayt sozlamalari — Seed:SampleContent o'chiq).</summary>
    public static async Task MigrateAndSeedAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await Seeder.SeedAsync(db, app.Configuration, app.Logger);
    }
}
