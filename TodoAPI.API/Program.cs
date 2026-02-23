using TodoAPI.API.Middleware;
using TodoAPI.Application;
using TodoAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Application (MediatR + FluentValidation)
builder.Services.AddApplication();

// Infrastructure (EF Core + SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=todoapi.db";
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

// Middleware pipeline (order matters!)
app.UseMiddleware<ExceptionMiddleware>();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<RequestLoggingMiddleware>();

app.MapControllers();

app.Run();