using EmployeeAPI;
using EmployeeAPI.Abstractions;
using EmployeeAPI.Employees;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton<IRepository<Employee>, EmployeeRepository>();

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

employeeRoute.MapGet(string.Empty, (IRepository<Employee> repository) => {
    return Results.Ok(repository.GetAll().Select(employee => new GetEmployeeResponse {
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

employeeRoute.MapGet("{id:int}", (int id, IRepository<Employee> repository) => {
    var employee = repository.GetById(id);
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

employeeRoute.MapPost(string.Empty, (CreateEmployeeRequest employeeRequest, IRepository<Employee> repository) => {
    var newEmployee = new Employee {
        FirstName = employeeRequest.FirstName,
        LastName = employeeRequest.LastName,
        SocialSecurityNumber = employeeRequest.SocialSecurityNumber,
        Address1 = employeeRequest.Address1,
        Address2 = employeeRequest.Address2,
        City = employeeRequest.City,
        State = employeeRequest.State,
        ZipCode = employeeRequest.ZipCode,
        PhoneNumber = employeeRequest.PhoneNumber,
        Email = employeeRequest.Email
    };
    repository.Create(newEmployee);
    return Results.Created($"/employees/{newEmployee.Id}", employeeRequest);
});

employeeRoute.MapPut("{id}", (UpdateEmployeeRequest employeeRequest, int id, IRepository<Employee> repository) => {
    var existingEmployee = repository.GetById(id);
    if (existingEmployee == null)
    {
        return Results.NotFound();
    }

    existingEmployee.Address1 = employeeRequest.Address1;
    existingEmployee.Address2 = employeeRequest.Address2;
    existingEmployee.City = employeeRequest.City;
    existingEmployee.State = employeeRequest.State;
    existingEmployee.ZipCode = employeeRequest.ZipCode;
    existingEmployee.PhoneNumber = employeeRequest.PhoneNumber;
    existingEmployee.Email = employeeRequest.Email;

    repository.Update(existingEmployee);
    return Results.Ok(existingEmployee);
});

app.UseHttpsRedirection();

app.Run();

public partial class Program {

}
