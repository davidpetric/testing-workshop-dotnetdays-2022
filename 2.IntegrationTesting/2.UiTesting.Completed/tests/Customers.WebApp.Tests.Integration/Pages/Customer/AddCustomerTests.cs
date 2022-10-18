using Bogus;
using Customers.Api.Repositories;
using Customers.WebApp.Data;
using Customers.WebApp.Repositories;
using FluentAssertions;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages.Customer;

[Collection("Test collection")]
public class AddCustomerTests
{
    private readonly TestingContext _testingContext;
    private readonly ICustomerRepository _customerRepository;
    private readonly Faker<CustomerDto> _customerGenerator = new Faker<CustomerDto>()
        .RuleFor(x => x.Id, Guid.NewGuid)
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.FullName, f => f.Person.FullName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));
    
    public AddCustomerTests(TestingContext testingContext)
    {
        _testingContext = testingContext;
        _customerRepository = new CustomerRepository(_testingContext.Database);
    }

    [Fact]
    public async Task Create_CreatesCustomer_WhenDataIsValid()
    {
        // Arrange
        var page = await _testingContext.Browser.NewPageAsync();
        await page.GotoAsync($"{TestingContext.AppUrl}/add-customer");

        var customer = _customerGenerator.Generate();
        _testingContext.GitHubApiServer.SetupUser(customer.GitHubUsername);
        
        // Act
        await page.Locator("id=fullname").FillAsync(customer.FullName);
        await page.Locator("id=email").FillAsync(customer.Email);
        await page.Locator("id=github-username").FillAsync(customer.GitHubUsername);
        await page.Locator("id=dob").FillAsync(customer.DateOfBirth.ToString("yyyy-MM-dd"));
        await page.Locator("text=Submit").ClickAsync();

        // Assert
        var href = await page.Locator("text='here'").GetAttributeAsync("href");
        var customerIdText = href!.Replace("/customer/", string.Empty);
        var customerId = Guid.Parse(customerIdText);

        var createdCustomer = await _customerRepository.GetAsync(customerId);

        createdCustomer.Should().BeEquivalentTo(customer, x => x.Excluding(p => p.Id));

        // Cleanup
        await page.CloseAsync();
        await _customerRepository.DeleteAsync(customerId);
    }
    
    [Fact]
    public async Task Create_ShowsErrorMessage_WhenEmailIsInvalid()
    {
        // Arrange
        var page = await _testingContext.Browser.NewPageAsync();
        await page.GotoAsync($"{TestingContext.AppUrl}/add-customer");
        
        // Act
        await page.Locator("id=email").FillAsync("notanemail");
        await page.Locator("id=github-username").FocusAsync();

        // Assert
        var errorCount = await page.Locator("text='Invalid email format'").CountAsync();
        errorCount.Should().Be(1);

        // Cleanup
        await page.CloseAsync();
    }
}
