using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Repositories;
using KnyazBarber.Api.Services;

namespace KnyazBarber.Api.Configurations;

public static class DependencyInjectionConfigurations
{
    public static void AddDependencyInjectionConfigurations(this WebApplicationBuilder builder)
    {
        builder.Services.AddSingleton(TimeProvider.System);
        builder.Services.AddSingleton<BusinessClock>();

        builder.Services.AddScoped(typeof(IBaseRepository<>), typeof(BaseRepository<>));

        builder.Services.AddSingleton<ITokenService, TokenService>();
        builder.Services.AddSingleton<IFileUploadService, FileUploadService>();
        builder.Services.AddScoped<IAuthService, AuthService>();
        builder.Services.AddScoped<IAvailabilityService, AvailabilityService>();
        builder.Services.AddScoped<IBookingService, BookingService>();
        builder.Services.AddScoped<IAppointmentService, AppointmentService>();
        builder.Services.AddScoped<IBarberManagementService, BarberManagementService>();
        builder.Services.AddScoped<IServiceCatalogService, ServiceCatalogService>();
        builder.Services.AddScoped<IGalleryService, GalleryService>();
        builder.Services.AddScoped<IBarberProfileService, BarberProfileService>();
        builder.Services.AddScoped<IReviewService, ReviewService>();
        builder.Services.AddScoped<ISettingsService, SettingsService>();
        builder.Services.AddScoped<IStatsService, StatsService>();
        builder.Services.AddScoped<IUserService, UserService>();
        builder.Services.AddScoped<ITimeOffService, TimeOffService>();
        builder.Services.AddScoped<IPublicSiteService, PublicSiteService>();
    }
}
