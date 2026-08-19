using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace VGS.RetailOS.Shared.Observability.Logging;

public static class LoggingExtensions
{
    public static IServiceCollection AddVgsStructuredLogging(this IServiceCollection services)
    {
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddJsonConsole(options =>
            {
                options.IncludeScopes = true;
                options.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ ";
                options.UseUtcTimestamp = true;
            });
        });

        return services;
    }

    public static IApplicationBuilder UseVgsRequestLogging(this IApplicationBuilder app)
    {
        return app.UseMiddleware<RequestLoggingMiddleware>();
    }
}
