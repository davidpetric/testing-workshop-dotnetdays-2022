using Bogus;
using Customers.Api.Repositories;
using Customers.WebApp.Data;
using Customers.WebApp.Repositories;
using FluentAssertions;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages.Customer;

[Collection("Test collection")]
public class UpdateCustomerTests
{
    private readonly TestingContext _testingContext;
    private readonly ICustomerRepository _customerRepository;
    private readonly Faker<CustomerDto> _customerGenerator = new Faker<CustomerDto>()
        .RuleFor(x => x.Id, Guid.NewGuid)
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.FullName, f => f.Person.FullName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));
    
    public UpdateCustomerTests(TestingContext testingContext)
    {
        _testingContext = testingContext;
        _customerRepository = new CustomerRepository(_testingContext.Database);
    }

    [Fact]
    public async Task Update_UpdatesCustomer_WhenDataIsValid()
    {
        // Arrange
        var page = await _testingContext.Browser.NewPageAsync();

        var customer = _customerGenerator.Generate();
        var newCustomer = _customerGenerator.Generate();
        _testingContext.GitHubApiServer.SetupUser(customer.GitHubUsername);
        await _customerRepository.CreateAsync(customer);
        
        await page.GotoAsync($"{TestingContext.AppUrl}/update-customer/{customer.Id}");
        
        // Act
        await page.Locator("id=fullname").FillAsync(newCustomer.FullName);
        await page.Locator("text=Submit").ClickAsync();

        // Assert
        var updatedCustomer = await _customerRepository.GetAsync(customer.Id);
        updatedCustomer!.FullName.Should().Be(newCustomer.FullName);

        // Cleanup
        await page.CloseAsync();
        await _customerRepository.DeleteAsync(customer.Id);
    }
}
