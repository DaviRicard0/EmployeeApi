using EmployeeAPI;
using EmployeeAPI.Employees;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);
var employees = new List<Employee>(){
    new() { Id = 1, FirstName = "John", LastName = "Doe", SocialSecurityNumber = "1234-23-1234" },
    new() { Id = 2, FirstName = "Jane", LastName = "Doe", SocialSecurityNumber = "1234-23-1234" }
};

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

var employeeRoute = app.MapGroup("employees");

// Configure the HTTP request pipeline.
/*if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}*/

employeeRoute.MapGet(string.Empty, () => {
    return Results.Ok(employees.Select(employee => new GetEmployeeResponse {
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    }));
});

employeeRoute.MapGet("{id:int}", (int id) => {
    var employee = employees.SingleOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }

    return Results.Ok(new GetEmployeeResponse {
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    });
});

employeeRoute.MapPost(string.Empty, ([FromBody] CreateEmployeeRequest employee) => {
    var newEmployee = new Employee {
        Id = employees.Max(e => e.Id) + 1,
        FirstName = employee.FirstName,
        LastName = employee.LastName,
        SocialSecurityNumber = employee.SocialSecurityNumber,
        Address1 = employee.Address1,
        Address2 = employee.Address2,
        City = employee.City,
        State = employee.State,
        ZipCode = employee.ZipCode,
        PhoneNumber = employee.PhoneNumber,
        Email = employee.Email
    };
    employees.Add(newEmployee);
    return Results.Created($"/employees/{newEmployee.Id}", employee);
});

employeeRoute.MapPut("{id}", ([FromBody] UpdateEmployeeRequest employee, int id) => {
    var existingEmployee = employees.SingleOrDefault(e => e.Id == id);
    if (existingEmployee == null)
    {
        return Results.NotFound();
    }

    existingEmployee.Address1 = employee.Address1;
    existingEmployee.Address2 = employee.Address2;
    existingEmployee.City = employee.City;
    existingEmployee.State =    employee.State;
    existingEmployee.ZipCode = employee.ZipCode;
    existingEmployee.PhoneNumber = employee.PhoneNumber;
    existingEmployee.Email = employee.Email;

    return Results.Ok(existingEmployee);
});

app.UseHttpsRedirection();

app.Run();

public partial class Program {

}
