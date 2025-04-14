using System.Text;
using EmployeeAPI;
using EmployeeAPI.Data;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Internal;
using Microsoft.IdentityModel.Tokens;
using Scalar.AspNetCore;
using Testcontainers.PostgreSql;

var postgreSqlContainer = new PostgreSqlBuilder().Build();
await postgreSqlContainer.StartAsync();

var builder = WebApplication.CreateBuilder(args);
var configuration = builder.Configuration;

/*builder.Services.AddSwaggerGen(options =>
{
    options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, "TheEmployeeAPI.xml"));
});*/
builder.Services.AddOpenApi();
builder.Services.AddProblemDetails();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddControllers(options => {
    options.Filters.Add<FluentValidationFilter>();
});

builder.Services.AddSingleton<ISystemClock, SystemClock>();

builder.Services.AddDbContext<AppDbContext>(options => {
    var conn = postgreSqlContainer.GetConnectionString();
    options.UseNpgsql(conn);
    options.UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking);
});

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(
        options =>
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidIssuer = configuration["Tokens:Issuer"],
                ValidAudience = configuration["Tokens:Issuer"],
                IssuerSigningKey =
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration["Tokens:Key"]!))
            };

            options.TokenValidationParameters = tokenValidationParameters;
        });

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
// builder.Services.AddOpenApi();

var app = builder.Build();

//kill container on shutdown
app.Lifetime.ApplicationStopping.Register(() => postgreSqlContainer.DisposeAsync());

using (var scope = app.Services.CreateScope()) {
    var services = scope.ServiceProvider;
    SeedData.MigrateAndSeed(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(options => {
        options.WithTitle("Employee Api");
    });
}

app.UseHttpsRedirection();
app.MapControllers();

app.Run();

public partial class Program {

}
