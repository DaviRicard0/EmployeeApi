using EmployeeAPI;
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

app.UseHttpsRedirection();

employeeRoute.MapGet(string.Empty, () => {
    foreach (var employee in employees)
    {
        employee.SocialSecurityNumber = null;
    }

    return Results.Ok(employees);
});

employeeRoute.MapGet("{id:int}", ([FromRoute] int id) => {
    var employee = employees.SingleOrDefault(e => e.Id == id);
    if (employee == null)
    {
        return Results.NotFound();
    }
    employee.SocialSecurityNumber = null;
    return Results.Ok(employee);
});

employeeRoute.MapPut("{id:int}", ([FromBody] Employee employee, [FromRoute] int id) => {
    var existingEmployee = employees.SingleOrDefault(e => e.Id == id);
    if (existingEmployee == null)
    {
        return Results.NotFound();
    }

    //existingEmployee.FirstName = employee.FirstName;
    //existingEmployee.LastName = employee.LastName;
    existingEmployee.Address1 = employee.Address1;
    existingEmployee.Address2 = employee.Address2;
    existingEmployee.City = employee.City;
    existingEmployee.State = employee.State;
    existingEmployee.ZipCode = employee.ZipCode;
    existingEmployee.PhoneNumber = employee.PhoneNumber;
    existingEmployee.Email = employee.Email;

    return Results.Ok(existingEmployee);
});

employeeRoute.MapPost(string.Empty,([FromBody] Employee employee, HttpContext context) => {
    employee.Id = employees.Max(e => e.Id) + 1;
    employees.Add(employee);
    return Results.Created($"/employees/{employee.Id}",employee);
});

app.Run();

public partial class Program {

}
