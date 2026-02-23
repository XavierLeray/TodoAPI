using Serilog;
using TodoAPI.API.Extensions;
using TodoAPI.Application;
using TodoAPI.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.Host.AddStructuredLogging(builder.Configuration);

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
builder.Services.AddInfrastructure(
    builder.Configuration.GetConnectionString("DefaultConnection") ?? "Data Source=todoapi.db",
    builder.Configuration.GetConnectionString("Redis"));

var app = builder.Build();

app.ConfigurePipeline();

app.Run();