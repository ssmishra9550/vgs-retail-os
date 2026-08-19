using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using VGS.RetailOS.Contracts.V1.Store.Requests;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Store;

public class StoreControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public StoreControllerTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task CreateStore_WithoutAuth_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var request = new CreateStoreRequest 
        { 
            OrganizationId = Guid.NewGuid(),
            Name = "Test Store" 
        };
        
        var response = await client.PostAsJsonAsync("/api/v1/stores", request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
