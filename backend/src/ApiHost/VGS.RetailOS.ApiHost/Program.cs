using VGS.RetailOS.Infrastructure;
using VGS.RetailOS.Shared.Errors;
using VGS.RetailOS.Shared.Observability.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVgsStructuredLogging();
builder.Services.AddVgsErrorHandling();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseVgsErrorHandling();
app.UseVgsRequestLogging();

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("live"),
    ResponseWriter = VGS.RetailOS.Shared.Observability.HealthChecks.HealthCheckResponseWriter.WriteResponse
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = r => r.Tags.Contains("ready"),
    ResponseWriter = VGS.RetailOS.Shared.Observability.HealthChecks.HealthCheckResponseWriter.WriteResponse
});

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    service = "VGS Retail OS API Host",
    status = "running"
}));

app.Run();
