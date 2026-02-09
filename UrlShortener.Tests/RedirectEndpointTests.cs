using FluentAssertions;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Xunit;
using UrlShortener.Api.Repository;

namespace UrlShortner.Tests;
public class RedirectEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public RedirectEndpointTests(WebApplicationFactory<Program> factory)
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "1 https://example.com abc");

        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                services.AddSingleton<IUrlRepository>(_ => new FileUrlRepository(file, NullLogger<FileUrlRepository>.Instance));
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task Redirects_WhenCodeExists()
    {
        var response = await _client.GetAsync("/abc");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location!.ToString().Should().Be("https://example.com/");
    }

    [Fact]
    public async Task Returns404_WhenCodeDoesNotExist()
    {
        var response = await _client.GetAsync("/missing");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}