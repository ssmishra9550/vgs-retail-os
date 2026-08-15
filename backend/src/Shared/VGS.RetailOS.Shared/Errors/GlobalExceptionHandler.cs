using System.Diagnostics;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using VGS.RetailOS.Shared.Errors.Exceptions;

namespace VGS.RetailOS.Shared.Errors;

public class GlobalExceptionHandler : IExceptionHandler
{
    private readonly IHostEnvironment _env;
    private readonly ILogger<GlobalExceptionHandler> _logger;

    public GlobalExceptionHandler(IHostEnvironment env, ILogger<GlobalExceptionHandler> logger)
    {
        _env = env;
        _logger = logger;
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        // The RequestLoggingMiddleware already logs the exception with context.
        // The framework's default ExceptionHandlerMiddleware also logs the exception.
        // We focus purely on transforming the exception into a standardized ProblemDetails response here.

        var statusCode = exception switch
        {
            ValidationException => StatusCodes.Status400BadRequest,
            NotFoundException => StatusCodes.Status404NotFound,
            ConflictException => StatusCodes.Status409Conflict,
            _ => StatusCodes.Status500InternalServerError
        };

        var errorCode = exception is BaseException baseEx ? baseEx.ErrorCode : "INTERNAL_ERROR";
        
        var title = exception switch
        {
            ValidationException => "Validation Error",
            NotFoundException => "Resource Not Found",
            ConflictException => "Conflict",
            _ => "An unexpected error occurred"
        };

        // Do not leak exception details in production for 500 errors
        var detail = (statusCode == 500 && !_env.IsDevelopment())
            ? "An unexpected internal server error occurred."
            : exception.Message;

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = title,
            Detail = detail,
            Instance = httpContext.Request.Path
        };

        var traceId = Activity.Current?.Id ?? httpContext.TraceIdentifier;
        problemDetails.Extensions["traceId"] = traceId;
        problemDetails.Extensions["code"] = errorCode;

        if (exception is ValidationException validationEx && validationEx.Errors.Count > 0)
        {
            problemDetails.Extensions["errors"] = validationEx.Errors;
        }

        // In Development, we can append stack trace for easier debugging
        if (_env.IsDevelopment() && statusCode == 500)
        {
            problemDetails.Extensions["stackTrace"] = exception.StackTrace;
        }

        httpContext.Response.StatusCode = statusCode;
        
        await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

        // Return true to signal that this exception is handled
        return true;
    }
}
