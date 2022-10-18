using Customers.Api.Database;
using DotNet.Testcontainers.Builders;
using DotNet.Testcontainers.Configurations;
using DotNet.Testcontainers.Containers;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Net.Http.Headers;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerApiFactory : WebApplicationFactory<IApiMarker>, IAsyncLifetime
{
    private readonly TestcontainerDatabase _dbContainer =
        new TestcontainersBuilder<PostgreSqlTestcontainer>()
            .WithDatabase(new PostgreSqlTestcontainerConfiguration
            {
                Database = "mydb",
                Username = "workshop",
                Password = "changeme"
            }).Build();

    public GitHubApiServer GitHubApiServer { get; } = new();
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
        });

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll(typeof(IDbConnectionFactory));
            services.AddSingleton<IDbConnectionFactory>(_ =>
                new NpgsqlConnectionFactory(_dbContainer.ConnectionString));
            
            services.AddHttpClient("GitHub", httpClient =>
            {
                httpClient.BaseAddress = new Uri(GitHubApiServer.Url);
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.Accept, "application/vnd.github.v3+json");
                httpClient.DefaultRequestHeaders.Add(
                    HeaderNames.UserAgent, $"Workshop-{Environment.MachineName}");
            });
        });
    }

    public async Task InitializeAsync()
    {
        GitHubApiServer.Start();
        await _dbContainer.StartAsync();
    }

    public new async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
        GitHubApiServer.Dispose();
    }
}
