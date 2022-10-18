using System.Net;
using System.Net.Http.Json;
using Bogus;
using Customers.Api.Contracts.Requests;
using Customers.Api.Contracts.Responses;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Customers.Api.Tests.Integration;

public class CustomerControllerTests : IClassFixture<CustomerApiFactory>, IAsyncLifetime
{
    private readonly HttpClient _client;
    private readonly List<Guid> _idsToDelete = new();

    private readonly GitHubApiServer _gitHubApiServer;

    private readonly Faker<CustomerRequest> _customerGenerator = new Faker<CustomerRequest>()
        .RuleFor(x => x.Email, f => f.Person.Email)
        .RuleFor(x => x.FullName, f => f.Person.FullName)
        .RuleFor(x => x.DateOfBirth, f => f.Person.DateOfBirth.Date)
        .RuleFor(x => x.GitHubUsername, f => f.Person.UserName.Replace(".", "").Replace("-", "").Replace("_", ""));

    public CustomerControllerTests(CustomerApiFactory customerApiFactory)
    {
        _client = customerApiFactory.CreateClient(new WebApplicationFactoryClientOptions
        {
            BaseAddress = new Uri("https://localhost:5001")
        });
        _gitHubApiServer = customerApiFactory.GitHubApiServer;
    }

    [Fact]
    public async Task Create_CreatesCustomer_WhenDetailsAreValid()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(request);

        response.Headers.Location.Should().Be($"https://localhost:5001/customers/{customerResponse!.Id}");

        // Cleanup
        _idsToDelete.Add(customerResponse.Id);
    }

    [Fact]
    public async Task Get_ReturnsCustomer_WhenCustomerExists()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", request);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.GetAsync($"customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();

        customerResponse.Should().BeEquivalentTo(createdCustomer);

        // Cleanup
        _idsToDelete.Add(customerResponse!.Id);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.GetAsync($"customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenTheEmailIsInvalid()
    {
        // Arrange
        var request = _customerGenerator.Clone()
            .RuleFor(x => x.Email, "nick")
            .Generate();

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails!.Errors["Email"].Should().Equal("nick is not a valid email address");
    }

    [Fact]
    public async Task GetAll_ReturnsAllCustomers_WhenCustomersExist()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", request);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.GetAsync("customers");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var customerResponse = await response.Content.ReadFromJsonAsync<GetAllCustomersResponse>();

        customerResponse!.Customers.Should().ContainEquivalentOf(createdCustomer)
            .And.HaveCount(1);

        // Cleanup
        _idsToDelete.Add(createdCustomer!.Id);
    }

    [Fact]
    public async Task Update_UpdatesCustomerDetails_WhenDetailsAreValid()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", request);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        var updateRequest = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(updateRequest.GitHubUsername);

        // Act
        var response = await _client.PutAsJsonAsync($"customers/{createdCustomer!.Id}", updateRequest);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var customerResponse = await response.Content.ReadFromJsonAsync<CustomerResponse>();
        customerResponse.Should().BeEquivalentTo(updateRequest);

        // Cleanup
        _idsToDelete.Add(customerResponse!.Id);
    }

    [Fact]
    public async Task Delete_DeletesCustomer_WhenCustomerExists()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupUser(request.GitHubUsername);

        var createCustomerHttpResponse = await _client.PostAsJsonAsync("customers", request);
        var createdCustomer = await createCustomerHttpResponse.Content.ReadFromJsonAsync<CustomerResponse>();

        // Act
        var response = await _client.DeleteAsync($"customers/{createdCustomer!.Id}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenCustomerDoesNotExist()
    {
        // Arrange
        var customerId = Guid.NewGuid();

        // Act
        var response = await _client.DeleteAsync($"customers/{customerId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenGitHubUserDoesNotExist()
    {
        // Arrange
        var request = _customerGenerator.Generate();

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        problemDetails!.Errors["Customer"].Should().Equal($"There is no GitHub user with username {request.GitHubUsername}");
    }

    [Fact]
    public async Task Create_ReturnsInternalServerError_WhenGitHubIsThrottled()
    {
        // Arrange
        var request = _customerGenerator.Generate();
        _gitHubApiServer.SetupThrottledUser(request.GitHubUsername);

        // Act
        var response = await _client.PostAsJsonAsync("customers", request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.InternalServerError);
    }

    public Task InitializeAsync() => Task.CompletedTask;

    public async Task DisposeAsync()
    {
        foreach (var id in _idsToDelete)
        {
            await _client.DeleteAsync($"customers/{id}");
        }
    }
}
