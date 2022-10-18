using Bogus;
using Customers.Api.Repositories;
using Customers.WebApp.Data;
using Customers.WebApp.Repositories;
using FluentAssertions;
using Xunit;

namespace Customers.WebApp.Tests.Integration.Pages.Customer;

[Collection("Test collection")]
public class DeleteCustomerTests
{
    private readonly TestingContext _testingContext;
    private readonly ICustomerRepository _customerRepository;
    private readonly Faker<CustomerDto> _customerGenerator = new Faker<CustomerDto>()
        .RuleFor(x => x.Id, Guid.NewGuid)
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.FullName, f => f.Person.FullName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));
    
    public DeleteCustomerTests(TestingContext testingContext)
    {
        _testingContext = testingContext;
        _customerRepository = new CustomerRepository(_testingContext.Database);
    }

    [Fact]
    public async Task Delete_DeletesCustomer_WhenCustomerExists()
    {
        // Arrange
        var page = await _testingContext.Browser.NewPageAsync();
        
        var customer = _customerGenerator.Generate();
        _testingContext.GitHubApiServer.SetupUser(customer.GitHubUsername);
        await _customerRepository.CreateAsync(customer);
        
        await page.GotoAsync($"{TestingContext.AppUrl}/customers");
        page.Dialog += (_, dialog) => dialog.AcceptAsync();

        // Act
        await page.Locator("text='Delete'").ClickAsync();

        // Assert
        var customerExists = await _customerRepository.GetAsync(customer.Id);
        customerExists.Should().BeNull();
        
        // Cleanup
        await page.CloseAsync();
    }
}
