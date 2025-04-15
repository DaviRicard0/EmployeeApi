using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeAPI.Data;
using EmployeeAPI.Entities;
using EmployeeAPI.Features.v1.Employees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAPI.Tests;

public class BasicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private const int EmployeeId = 1;
    private readonly string _adminRole = "Admin";

    public BasicTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private async Task AuthenticateClientAsync(HttpClient client)
    {
        var resp = await client.PostAsJsonAsync("api/v1/auth/generateAVeryInsecureToken_pleasedontusethisever", new
        {
            role = _adminRole, 
            username = "test@test.com"
        });
        resp.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await resp.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task GetAllEmployees_ShouldReturnOkResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var response = await client.GetAsync("api/v1/employees");

        Assert.True(response.IsSuccessStatusCode, await GetErrorMessage(response));
        var employees = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponse>>()
                             ?? throw new ArgumentNullException(nameof(IEnumerable<GetEmployeeResponse>));
        Assert.NotEmpty(employees);
    }

    [Fact]
    public async Task GetAllEmployees_WithFilter_ShouldReturnOneResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var response = await client.GetAsync("api/v1/employees?FirstNameContains=John");

        Assert.True(response.IsSuccessStatusCode, await GetErrorMessage(response));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employeesInDb = db.Employees.ToList();

        var employees = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponse>>()
                             ?? throw new ArgumentNullException(nameof(IEnumerable<GetEmployeeResponse>));
        Assert.Single(employees);
    }

    [Fact]
    public async Task GetEmployeeById_ShouldReturnOkResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var response = await client.GetAsync($"api/v1/employees/{EmployeeId}");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_ShouldReturnCreatedResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var employee = new Employee { FirstName = "Tom", LastName = "Doe", SocialSecurityNumber = "1111-11-1111" };
        var response = await client.PostAsJsonAsync("api/v1/employees", employee);

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_WithInvalidData_ShouldReturnBadRequest()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var invalidEmployee = new CreateEmployeeRequest();

        var response = await client.PostAsJsonAsync("api/v1/employees", invalidEmployee);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("FirstName", problemDetails.Errors.Keys);
        Assert.Contains("LastName", problemDetails.Errors.Keys);
        Assert.Contains("'First Name' must not be empty.", problemDetails.Errors["FirstName"]);
        Assert.Contains("'Last Name' must not be empty.", problemDetails.Errors["LastName"]);
    }

    [Fact]
    public async Task UpdateEmployee_ShouldReturnOkResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var updatedEmployee = new Employee { FirstName = "John", LastName = "Doe", Address1 = "123 Main Smoot" };
        var response = await client.PutAsJsonAsync($"api/v1/employees/{EmployeeId}", updatedEmployee);

        Assert.True(response.IsSuccessStatusCode, await GetErrorMessage(response));

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = await db.Employees.FindAsync(EmployeeId);
        Assert.NotNull(employee);
        Assert.Equal("123 Main Smoot", employee.Address1);
    }

    [Fact]
    public async Task UpdateEmployee_WithInvalidId_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var response = await client.PutAsJsonAsync("api/v1/employees/0", new Employee { FirstName = "John", LastName = "Doe" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_WithInvalidData_ShouldReturnBadRequest()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var invalidEmployee = new UpdateEmployeeRequest();

        var response = await client.PutAsJsonAsync($"api/v1/employees/{EmployeeId}", invalidEmployee);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("Address1", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task GetBenefitsForEmployee_ShouldReturnOkResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var response = await client.GetAsync($"api/v1/employees/{EmployeeId}/benefits");

        response.EnsureSuccessStatusCode();
        
        var benefits = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponseEmployeeBenefit>>()
                              ?? throw new ArgumentNullException(nameof(IEnumerable<GetEmployeeResponseEmployeeBenefit>));
        
        Assert.Equal(2, benefits.Count());
    }

    [Fact]
    public async Task DeleteEmployee_ShouldReturnNoContentResult()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);

        var newEmployee = new Employee { FirstName = "Meow", LastName = "Garita" };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Employees.Add(newEmployee);
            await db.SaveChangesAsync();
        }

        var response = await client.DeleteAsync($"api/v1/employees/{newEmployee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_WithInvalidId_ShouldReturnNotFound()
    {
        var client = _factory.CreateClient();
        await AuthenticateClientAsync(client);
        var response = await client.DeleteAsync("api/v1/employees/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    private static async Task<string> GetErrorMessage(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return $"Failed with status code {response.StatusCode}: {content}";
    }
}
