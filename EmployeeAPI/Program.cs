using System.Text;
using Asp.Versioning;
using EmployeeAPI.Data;
using EmployeeAPI.Helpers;
using FluentValidation;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
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

builder.Services.AddHealthChecks()
    .AddNpgSql(postgreSqlContainer.GetConnectionString(),name:"postgresql",tags: ["db","data","sql"]);

builder.Services.AddHealthChecksUI(options => {
    options.AddHealthCheckEndpoint("Infrastructure", "/health");
}).AddInMemoryStorage();

builder.Services.AddLimiterRules();

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
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
        })
    .AddGoogle(options =>
    {
        options.ClientId = configuration["Authentication:Google:ClientId"]!;
        options.ClientSecret = configuration["Authentication:Google:ClientSecret"]!;
        options.SignInScheme = IdentityConstants.ExternalScheme;
    });

builder.Services.AddApiVersioning(options => {
    options.DefaultApiVersion = new ApiVersion(1);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
}).AddApiExplorer(options => {
    options.GroupNameFormat = "'v'V";
    options.SubstituteApiVersionInUrl = true;
});

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

app.MapHealthChecks("/health",
    new(){
        ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
    }
);
app.UseHealthChecksUI(config => config.UIPath = "/health-ui");

app.Run();

public partial class Program {

}
