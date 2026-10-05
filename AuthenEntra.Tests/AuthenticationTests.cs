using System.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.OpenIdConnect;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.IdentityModel.Protocols.OpenIdConnect;
using Microsoft.IdentityModel.Protocols;
using Microsoft.Extensions.DependencyInjection;

namespace AuthenEntra.Tests;

public sealed class AuthenticationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient client;

    public AuthenticationTests(WebApplicationFactory<Program> factory)
    {
        client = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Development");
            builder.ConfigureTestServices(services =>
            {
                services.PostConfigure<OpenIdConnectOptions>(OpenIdConnectDefaults.AuthenticationScheme, options =>
                {
                    var configuration = new OpenIdConnectConfiguration
                    {
                        AuthorizationEndpoint = "https://login.microsoftonline.com/test/oauth2/v2.0/authorize",
                        EndSessionEndpoint = "https://login.microsoftonline.com/test/oauth2/v2.0/logout"
                    };
                    options.Configuration = configuration;
                    options.ConfigurationManager = new StaticConfigurationManager<OpenIdConnectConfiguration>(configuration);
                });
            });
        }).CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
    }

    [Fact]
    public async Task HomePage_ReturnsLoginUi()
    {
        var response = await client.GetAsync("/");
        var content = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Login ด้วย Microsoft Account", content);
    }

    [Fact]
    public async Task SwaggerJson_IsAvailable()
    {
        var response = await client.GetAsync("/swagger/v1/swagger.json");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("/api/Users/me", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Login_RedirectsToMicrosoftIdentity()
    {
        var response = await client.GetAsync("/account/login?returnUrl=/");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("login.microsoftonline.com", response.Headers.Location?.ToString());
    }

    [Fact]
    public async Task CurrentUser_RequiresLogin()
    {
        var response = await client.GetAsync("/api/users/me");

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);
        Assert.Contains("login.microsoftonline.com", response.Headers.Location?.ToString());
    }
}