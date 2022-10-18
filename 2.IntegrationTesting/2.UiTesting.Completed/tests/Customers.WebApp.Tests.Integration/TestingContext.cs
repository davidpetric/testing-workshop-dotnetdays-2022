using Customers.WebApp.Database;
using Ductus.FluentDocker.Builders;
using Ductus.FluentDocker.Model.Common;
using Ductus.FluentDocker.Services;
using Microsoft.Playwright;
using Xunit;

namespace Customers.WebApp.Tests.Integration;

public class TestingContext : IAsyncLifetime
{
    public const string AppUrl = "https://localhost:7780";
    
    private static readonly string DockerComposeFile =
        Path.Combine(Directory.GetCurrentDirectory(), (TemplateString)"../../../docker-compose.integration.yml");
    
    private readonly ICompositeService _dockerService = new Builder()
        .UseContainer()
        .UseCompose()
        .FromFile(DockerComposeFile)
        .RemoveOrphans()
        .WaitForHttp("test-app", AppUrl)
        .Build();

    private IPlaywright _playwright;

    public IDbConnectionFactory Database { get; private set; }

    public IBrowserContext Browser { get; private set; }

    public GitHubApiServer GitHubApiServer { get; } = new();

    public async Task InitializeAsync()
    {
        Database = new NpgsqlConnectionFactory(
            "Server=localhost;Port=5435;Database=mydb;User ID=workshop;Password=changeme;");
        
        GitHubApiServer.Start(9850);
        _dockerService.Start();
     
        _playwright = await Playwright.CreateAsync();
        var browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
        {
            SlowMo = 50,
            Headless = true
        });
        
        Browser = await browser.NewContextAsync(new BrowserNewContextOptions
        {
            IgnoreHTTPSErrors = true
        });
    }

    public async Task DisposeAsync()
    {
        await Browser.DisposeAsync();
        _playwright.Dispose();
        _dockerService.Dispose();
        GitHubApiServer.Dispose();
    }
}
