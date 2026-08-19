using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using VGS.RetailOS.Contracts.V1.Organization.Requests;
using VGS.RetailOS.Contracts.V1.Organization.Responses;
using Xunit;

namespace VGS.RetailOS.Tests.Integration.Organization;

public class OrganizationControllerTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public OrganizationControllerTests(WebApplicationFactory<Program> factory)
    {
        // Simple test factory, assumes DB and everything is set up via Program.cs
        _factory = factory;
    }

    [Fact]
    public async Task CreateOrganization_WithoutAuth_ReturnsUnauthorized()
    {
        var client = _factory.CreateClient();
        
        var request = new CreateOrganizationRequest { Name = "Test Org" };
        
        var response = await client.PostAsJsonAsync("/api/v1/organizations", request);
        
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
