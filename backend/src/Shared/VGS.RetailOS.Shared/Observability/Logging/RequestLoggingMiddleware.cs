using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace VGS.RetailOS.Shared.Observability.Logging;

public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var startTime = Stopwatch.GetTimestamp();
        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;
        
        // Setup shared logging scope properties
        var scopeProperties = new Dictionary<string, object>
        {
            ["TraceId"] = traceId,
            ["Method"] = context.Request.Method,
            ["Path"] = context.Request.Path
        };

        // TenantId, UserId, etc., can be added to this scope in future auth/tenant middlewares.
        using (_logger.BeginScope(scopeProperties))
        {
            try
            {
                await _next(context);

                var elapsed = Stopwatch.GetElapsedTime(startTime);
                _logger.LogInformation("HTTP Request completed: {StatusCode} in {ElapsedMilliseconds}ms", 
                    context.Response.StatusCode, 
                    elapsed.TotalMilliseconds);
            }
            catch (Exception ex)
            {
                var elapsed = Stopwatch.GetElapsedTime(startTime);
                _logger.LogError(ex, "Unhandled exception during HTTP Request: {ExceptionType} - {ExceptionMessage} in {ElapsedMilliseconds}ms", 
                    ex.GetType().Name,
                    ex.Message,
                    elapsed.TotalMilliseconds);
                
                throw;
            }
        }
    }
}
