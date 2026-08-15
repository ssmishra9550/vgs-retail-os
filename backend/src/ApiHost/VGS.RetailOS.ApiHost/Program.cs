using VGS.RetailOS.Infrastructure;
using VGS.RetailOS.Shared.Observability.Logging;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddVgsStructuredLogging();
builder.Services.AddInfrastructure(builder.Configuration);

var app = builder.Build();

app.UseVgsRequestLogging();

app.UseHttpsRedirection();

app.MapGet("/", () => Results.Ok(new
{
    service = "VGS Retail OS API Host",
    status = "running"
}));

app.Run();
