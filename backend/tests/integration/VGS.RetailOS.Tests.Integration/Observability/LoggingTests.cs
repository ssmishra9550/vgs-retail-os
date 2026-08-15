using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using VGS.RetailOS.Shared.Observability.Logging;

namespace VGS.RetailOS.Tests.Integration.Observability;

public class LoggingTests
{
    [Fact]
    public async Task RequestLoggingMiddleware_Should_Complete_Successfully()
    {
        // Arrange
        var hostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddVgsStructuredLogging();
            })
            .Configure(app =>
            {
                app.UseVgsRequestLogging();
                app.Run(async context =>
                {
                    await context.Response.WriteAsync("OK");
                });
            });

        using var server = new TestServer(hostBuilder);
        var client = server.CreateClient();

        // Act
        var response = await client.GetAsync("/test-logging-route");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        Assert.Equal("OK", content);
        // We know it didn't crash. Structured log verification could be done with a mock logger provider,
        // but here we ensure the pipeline configuration is valid.
    }

    [Fact]
    public async Task RequestLoggingMiddleware_Should_Catch_And_Rethrow_Exceptions()
    {
        // Arrange
        var hostBuilder = new WebHostBuilder()
            .ConfigureServices(services =>
            {
                services.AddVgsStructuredLogging();
            })
            .Configure(app =>
            {
                app.UseVgsRequestLogging();
                app.Run(context =>
                {
                    throw new InvalidOperationException("Test exception for logging");
                });
            });

        using var server = new TestServer(hostBuilder);
        var client = server.CreateClient();

        // Act & Assert
        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => client.GetAsync("/error-route"));
        Assert.Equal("Test exception for logging", ex.Message);
    }
}
