using TodoAPI.Application;
using TodoAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Controllers
builder.Services.AddControllers();

// Application (MediatR)
builder.Services.AddApplication();

// Infrastructure (EF Core + SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=todoapi.db";
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.MapControllers();

app.Run();