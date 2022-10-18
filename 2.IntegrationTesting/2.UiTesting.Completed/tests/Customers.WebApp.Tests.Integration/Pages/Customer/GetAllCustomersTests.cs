using Bogus;
using Customers.Api.Repositories;
using Customers.WebApp.Data;
using Customers.WebApp.Repositories;
using FluentAssertions;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages.Customer;

[Collection("Test collection")]
public class GetAllCustomersTests
{
    private readonly TestingContext _testingContext;
    private readonly ICustomerRepository _customerRepository;
    private readonly Faker<CustomerDto> _customerGenerator = new Faker<CustomerDto>()
        .RuleFor(x => x.Id, Guid.NewGuid)
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.FullName, f => f.Person.FullName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));
    
    public GetAllCustomersTests(TestingContext testingContext)
    {
        _testingContext = testingContext;
        _customerRepository = new CustomerRepository(_testingContext.Database);
    }

    [Fact]
    public async Task GetAll_ContainsCustomer_WhenCustomerExists()
    {
        // Arrange
        var page = await _testingContext.Browser.NewPageAsync();
        
        var customer = _customerGenerator.Generate();
        _testingContext.GitHubApiServer.SetupUser(customer.GitHubUsername);
        await _customerRepository.CreateAsync(customer);

        // Act
        await page.GotoAsync($"{TestingContext.AppUrl}/customers");

        // Assert
        var tableRow = page.Locator($"tr:has-text('{customer.Email}')");
        var tableCells = tableRow.Locator("td");

        var fullName = await tableCells.Nth(0).InnerTextAsync();
        var email = await tableCells.Nth(1).InnerTextAsync();
        var githubUsername = await tableCells.Nth(2).InnerTextAsync();
        var dateOfBirth = await tableCells.Nth(3).InnerTextAsync();

        fullName.Should().Be(customer.FullName);
        email.Should().Be(customer.Email);
        githubUsername.Should().Be(customer.GitHubUsername);
        dateOfBirth.Should().Be(customer.DateOfBirth.ToString("dd/MM/yyyy"));
        
        // Cleanup
        await page.CloseAsync();
        await _customerRepository.DeleteAsync(customer.Id);
    }
}
