using System.Net;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MiniShop.Core.Interfaces;
using MiniShop.Infrastructure;

namespace MiniShop.Tests;

public class ApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IOrderRepository>();
            services.AddSingleton<IOrderRepository, InMemoryOrderRepository>();
        });
    }
}

public class ApiTests : IClassFixture<ApiFactory>
{
    private const string SeedId = "000000000000000000000001";
    private readonly HttpClient _client;

    public ApiTests(ApiFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task GetOrders_InvalidCustomerId_Returns400()
    {
        var response = await _client.GetAsync("/api/orders?customerId=abc");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetOrders_ValidCustomerId_ReturnsTheOrder()
    {
        var response = await _client.GetAsync($"/api/orders?customerId={SeedId}");
        var body = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(SeedId, body);
    }

    [Fact]
    public async Task Cancel_Twice_FirstOkThenConflict()
    {
        var first = await _client.PostAsync($"/api/orders/{SeedId}/cancel", null);
        var second = await _client.PostAsync($"/api/orders/{SeedId}/cancel", null);

        Assert.Equal(HttpStatusCode.OK, first.StatusCode);
        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Cancel_UnknownOrder_Returns404AsProblemJson()
    {
        var response = await _client.PostAsync("/api/orders/000000000000000000000099/cancel", null);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task GetOrders_PageSizeTooLarge_Returns400()
    {
        var response = await _client.GetAsync($"/api/orders?customerId={SeedId}&pageSize=500");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
    [Fact]
    public async Task HealthLive_Returns200()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}