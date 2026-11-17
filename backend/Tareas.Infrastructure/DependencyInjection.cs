using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tareas.Domain;

namespace Tareas.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddTaskStorage(this IServiceCollection services)
    {
        services.AddDbContext<AppDbContext>((provider, options) =>
        {
            var connection = provider.GetRequiredService<IConfiguration>().GetConnectionString("DefaultConnection");
            if (string.IsNullOrWhiteSpace(connection))
                throw new InvalidOperationException("Configure ConnectionStrings__DefaultConnection.");

            options.UseSqlServer(connection, sql =>
                sql.MigrationsHistoryTable("__EFMigrationsHistory", AppDbContext.Schema));
        });

        return services.AddScoped<ITaskRepository, TaskRepository>();
    }
}
