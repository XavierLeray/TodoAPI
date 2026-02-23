using TodoAPI.API.Extensions;
using TodoAPI.Application;
using TodoAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// API
builder.Services.AddControllers();
builder.Services.AddApiVersioningConfiguration();
builder.Services.AddSwaggerDocumentation();
builder.Services.AddHealthChecks();

// Authentication
builder.Services.AddJwtAuthentication(builder.Configuration);

// Application (MediatR + FluentValidation)
builder.Services.AddApplication();

// Infrastructure (EF Core + SQLite + Redis)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=todoapi.db";
var redisConnectionString = builder.Configuration.GetConnectionString("Redis");
builder.Services.AddInfrastructure(connectionString, redisConnectionString);

var app = builder.Build();

app.ConfigurePipeline();

app.Run();