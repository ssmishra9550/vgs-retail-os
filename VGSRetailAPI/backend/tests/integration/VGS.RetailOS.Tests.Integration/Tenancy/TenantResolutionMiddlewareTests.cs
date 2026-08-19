using System.Net;
using System.Net.Http.Headers;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Tenancy;

[Collection("IntegrationTestCollection")]
public class TenantResolutionMiddlewareTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TenantResolutionMiddlewareTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task GetHealthLive_ShouldBypassTenantResolution_AndReturnOk()
    {
        // Act
        var response = await _client.GetAsync("/health/live");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetRoot_ShouldReturnBadRequest_WhenNoTenantIsProvided()
    {
        // Act
        var response = await _client.GetAsync("/");

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var content = await response.Content.ReadAsStringAsync();
        using var document = JsonDocument.Parse(content);
        
        var title = document.RootElement.GetProperty("title").GetString();
        Assert.Equal("Tenant Not Found", title);
    }

    [Fact]
    public async Task GetRoot_ShouldReturnOk_WhenTenantIsProvidedViaHeader()
    {
        // Arrange
        var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.Add("X-Tenant-Id", "tenant-test");

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
