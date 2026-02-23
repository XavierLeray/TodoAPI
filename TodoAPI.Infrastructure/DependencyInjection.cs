using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TodoAPI.Domain.Ports;
using TodoAPI.Infrastructure.Data;
using TodoAPI.Infrastructure.Repositories;

namespace TodoAPI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<ITodoRepository, EfTodoRepository>();

        return services;
    }
}