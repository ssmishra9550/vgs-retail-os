using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace VGS.RetailOS.Shared.Errors;

public static class ErrorHandlingExtensions
{
    public static IServiceCollection AddVgsErrorHandling(this IServiceCollection services)
    {
        services.AddProblemDetails();
        services.AddExceptionHandler<GlobalExceptionHandler>();
        
        return services;
    }

    public static IApplicationBuilder UseVgsErrorHandling(this IApplicationBuilder app)
    {
        app.UseExceptionHandler();
        return app;
    }
}
