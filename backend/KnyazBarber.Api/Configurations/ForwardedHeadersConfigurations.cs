using Microsoft.AspNetCore.HttpOverrides;

namespace KnyazBarber.Api.Configurations;

public static class ForwardedHeadersConfigurations
{
    /// <summary>
    /// Prod: API faqat nginx ortida — mijozning haqiqiy IP'si X-Forwarded-For dan olinadi (rate limiting uchun).
    /// nginx bu sarlavhani o'zi qayta yozadi, shuning uchun mijoz uni soxtalashtira olmaydi.
    /// </summary>
    public static void UseForwardedHeadersConfigurations(this WebApplication app)
    {
        if (!app.Configuration.GetValue<bool>("ReverseProxy:TrustForwardedHeaders")) return;

        var forwarded = new ForwardedHeadersOptions {ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto};
        forwarded.KnownIPNetworks.Clear();
        forwarded.KnownProxies.Clear();
        app.UseForwardedHeaders(forwarded);
    }
}
