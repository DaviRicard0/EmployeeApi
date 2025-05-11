var builder = DistributedApplication.CreateBuilder(args);

builder.AddProject<Projects.EmployeeAPI>("employee_api");

builder.Build().Run();
