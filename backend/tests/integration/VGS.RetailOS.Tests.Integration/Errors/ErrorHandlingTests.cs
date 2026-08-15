using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using VGS.RetailOS.Shared.Errors;
using VGS.RetailOS.Shared.Errors.Exceptions;
using VGS.RetailOS.Shared.Observability.Logging;

namespace VGS.RetailOS.Tests.Integration.Errors;

public class ErrorHandlingTests
{
    private TestServer CreateServer(string environment = "Production")
    {
        var hostBuilder = new WebHostBuilder()
            .UseEnvironment(environment)
            .ConfigureServices(services =>
            {
                services.AddVgsStructuredLogging();
                services.AddVgsErrorHandling();
                services.AddRouting();
            })
            .Configure(app =>
            {
                app.UseVgsErrorHandling();
                app.UseVgsRequestLogging();
                app.UseRouting();
                
                app.UseEndpoints(endpoints =>
                {
                    endpoints.MapGet("/success", () => Results.Ok("OK"));
                    endpoints.MapGet("/validation", (HttpContext ctx) => { throw new ValidationException("Invalid input", new Dictionary<string, string[]> { { "Field", new[] { "Required" } } }); });
                    endpoints.MapGet("/notfound", (HttpContext ctx) => { throw new NotFoundException("Item not found"); });
                    endpoints.MapGet("/conflict", (HttpContext ctx) => { throw new ConflictException("Already exists"); });
                    endpoints.MapGet("/unhandled", (HttpContext ctx) => { throw new InvalidOperationException("Secret db connection string failed"); });
                });
            });

        return new TestServer(hostBuilder);
    }

    [Fact]
    public async Task Successful_Request_Should_Remain_Unaffected()
    {
        using var server = CreateServer();
        var client = server.CreateClient();

        var response = await client.GetAsync("/success");

        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("\"OK\"", content);
    }

    [Fact]
    public async Task ValidationException_Should_Return_400_ProblemDetails()
    {
        using var server = CreateServer();
        var client = server.CreateClient();

        var response = await client.GetAsync("/validation");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Assert.NotNull(problem);
        Assert.Equal(400, problem.Status);
        Assert.Equal("Validation Error", problem.Title);
        Assert.Equal("Invalid input", problem.Detail);
        Assert.Equal("VALIDATION_ERROR", problem.Extensions["code"]?.ToString());
        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.True(problem.Extensions.ContainsKey("errors"));
    }

    [Fact]
    public async Task NotFoundException_Should_Return_404_ProblemDetails()
    {
        using var server = CreateServer();
        var client = server.CreateClient();

        var response = await client.GetAsync("/notfound");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Assert.NotNull(problem);
        Assert.Equal(404, problem.Status);
        Assert.Equal("Resource Not Found", problem.Title);
        Assert.Equal("Item not found", problem.Detail);
        Assert.Equal("NOT_FOUND", problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task ConflictException_Should_Return_409_ProblemDetails()
    {
        using var server = CreateServer();
        var client = server.CreateClient();

        var response = await client.GetAsync("/conflict");

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Assert.NotNull(problem);
        Assert.Equal(409, problem.Status);
        Assert.Equal("Conflict", problem.Title);
        Assert.Equal("Already exists", problem.Detail);
        Assert.Equal("CONFLICT", problem.Extensions["code"]?.ToString());
    }

    [Fact]
    public async Task UnhandledException_In_Production_Should_Hide_Details()
    {
        using var server = CreateServer("Production");
        var client = server.CreateClient();

        var response = await client.GetAsync("/unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        var problem = await response.Content.ReadFromJsonAsync<ProblemDetails>();
        
        Assert.NotNull(problem);
        Assert.Equal(500, problem.Status);
        Assert.Equal("An unexpected error occurred", problem.Title);
        Assert.Equal("An unexpected internal server error occurred.", problem.Detail);
        Assert.NotNull(problem.Detail);
        Assert.DoesNotContain("Secret", problem.Detail);
        Assert.Equal("INTERNAL_ERROR", problem.Extensions["code"]?.ToString());
        Assert.True(problem.Extensions.ContainsKey("traceId"));
        Assert.False(problem.Extensions.ContainsKey("stackTrace"));
    }
    
    [Fact]
    public async Task UnhandledException_In_Development_Should_Include_Details()
    {
        using var server = CreateServer("Development");
        var client = server.CreateClient();

        var response = await client.GetAsync("/unhandled");

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);
        
        // Parse raw string to check for stackTrace since ProblemDetails might not map all extensions perfectly with ReadFromJsonAsync in this simple check
        var contentString = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(contentString);
        
        Assert.Equal(500, doc.RootElement.GetProperty("status").GetInt32());
        Assert.Equal("Secret db connection string failed", doc.RootElement.GetProperty("detail").GetString());
        Assert.True(doc.RootElement.TryGetProperty("stackTrace", out var _));
    }
}
