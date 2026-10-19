using KnyazBarber.Api.Configurations;
using KnyazBarber.Api.Helpers;
using KnyazBarber.Api.Middlewares;

var builder = WebApplication.CreateBuilder(args);

builder.AddDatabaseConfigurations();
builder.AddJwtConfigurations();
builder.AddRateLimitConfigurations();
builder.AddCorsConfigurations();
builder.AddTelegramConfigurations();
builder.AddDependencyInjectionConfigurations();

builder.Services.AddProblemDetails();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

var app = builder.Build();

// Konsol buyruqlari: reset-password, create-admin, set-telegram (Helpers/CliCommands.cs)
if (await CliCommands.TryRunAsync(app, args)) return;

if (app.Environment.IsDevelopment()) app.MapOpenApi();

await app.MigrateAndSeedAsync();

app.UseForwardedHeadersConfigurations();
app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseStatusCodePages();
app.UseStaticFiles();
app.UseCors(CorsConfigurations.FrontendPolicy);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
