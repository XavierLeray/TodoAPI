using TodoAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Infrastructure (EF Core + SQLite)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=todoapi.db";
builder.Services.AddInfrastructure(connectionString);

var app = builder.Build();

app.MapGet("/", () => "TodoAPI is running");

app.Run();