using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using EmployeeAPI.Employees;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace EmployeeAPI.Tests;

public class BasicTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;
    private int _employeeId = 1;

    public BasicTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetAllEmployees_ReturnsOkResult(){
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.GetAsync("api/employees");

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            throw new Exception($"Failed to get employees: {content}");
        }

        var employees = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponse>>() ??
             throw new ArgumentNullException(nameof(IEnumerable<GetEmployeeResponse>));
        Assert.NotEmpty(employees);
    }

    [Fact]
    public async Task GetAllEmployees_WithFilter_ReturnsOneResult()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.GetAsync("api/employees?FirstNameContains=John");

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Failed to get employees: {content}");
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var allEmployees = db.Employees.ToList();

        var employees = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponse>>() ??
            throw new ArgumentNullException(nameof(IEnumerable<GetEmployeeResponse>));
        Assert.Single(employees);
    }

    [Fact]
    public async Task GetEmployeeById_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.GetAsync("api/employees/1");

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_ReturnsCreatedResult()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.PostAsJsonAsync("api/employees", new Employee { FirstName = "Tom", LastName = "Doe", SocialSecurityNumber = "1111-11-1111" });

        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task CreateEmployee_ReturnsBadRequestResult()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var invalidEmployee = new CreateEmployeeRequest(); // Empty object to trigger validation errors

        // Act
        var response = await client.PostAsJsonAsync("api/employees", invalidEmployee);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("FirstName", problemDetails.Errors.Keys);
        Assert.Contains("LastName", problemDetails.Errors.Keys);
        Assert.Contains("'First Name' must not be empty.", problemDetails.Errors["FirstName"]);
        Assert.Contains("'Last Name' must not be empty.", problemDetails.Errors["LastName"]);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsOkResult()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.PutAsJsonAsync("api/employees/1", new Employee { 
            FirstName = "John", 
            LastName = "Doe", 
            Address1 = "123 Main Smoot" 
        });

        if (!response.IsSuccessStatusCode)
        {
            var content = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Failed to update employee: {content}");
        }

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var employee = await db.Employees.FindAsync(1) ??
            throw new ArgumentNullException(nameof(DbSet<Employee>));;
        Assert.Equal("123 Main Smoot", employee.Address1);
        Assert.Equal(CustomWebApplicationFactory.SystemClock.UtcNow.UtcDateTime, employee.LastModifiedOn);
        Assert.Equal("test@test.com", employee.LastModifiedBy);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsNotFoundForNonExistentEmployee()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.PutAsJsonAsync("api/employees/0", new Employee { FirstName = "John", LastName = "Doe", SocialSecurityNumber = "1111-11-1111" });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task UpdateEmployee_ReturnsBadRequestWhenAddress()
    {
        // Arrange
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var invalidEmployee = new UpdateEmployeeRequest(); // Empty object to trigger validation errors

        // Act
        var response = await client.PutAsJsonAsync($"api/employees/{_employeeId}", invalidEmployee);

        // Assert
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Contains("Address1", problemDetails.Errors.Keys);
    }

    [Fact]
    public async Task GetBenefitsForEmployee_ReturnsOkResult()
    {
        // Act
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.GetAsync($"api/employees/{_employeeId}/benefits");

        // Assert
        response.EnsureSuccessStatusCode();
        
        var benefits = await response.Content.ReadFromJsonAsync<IEnumerable<GetEmployeeResponseEmployeeBenefit>>() ??
            throw new ArgumentNullException(nameof(IEnumerable<GetEmployeeResponseEmployeeBenefit>));
        
        Assert.Equal(2, benefits.Count());
    }
    
    [Fact]
    public async Task DeleteEmployee_ReturnsNoContentResult()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");

        var newEmployee = new Employee { FirstName = "Meow", LastName = "Garita" };
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Employees.Add(newEmployee);
            await db.SaveChangesAsync();
        }

        var response = await client.DeleteAsync($"api/employees/{newEmployee.Id}");

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);
    }

    [Fact]
    public async Task DeleteEmployee_ReturnsNotFoundResult()
    {
        var client = _factory.CreateClient();
        await AddAuthorizationToClientForRoleAsync(client, "Admin");
        var response = await client.DeleteAsync("api/employees/99999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    protected async Task AddAuthorizationToClientForRoleAsync(HttpClient client, string role)
    {
        var resp = await client.PostAsJsonAsync("api/auth/generateAVeryInsecureToken_pleasedontusethisever", new
        {
            role, username = "test@test.com"
        });
        resp.EnsureSuccessStatusCode();
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", await resp.Content.ReadAsStringAsync());
    }
}