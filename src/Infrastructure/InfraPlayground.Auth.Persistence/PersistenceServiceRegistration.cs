using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Application.Common.Repositories.Books;
using InfraPlayground.Auth.Persistence.Contexts;
using InfraPlayground.Auth.Persistence.Repositories;
using InfraPlayground.Auth.Persistence.Repositories.Books;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfraPlayground.Auth.Persistence;

public static class PersistenceServiceRegistration
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<InfraPlaygroundAuthDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection")
                                 ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is required.");

            options.UseNpgsql(connectionString);
        });

        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
        services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
        services.AddScoped<IBookReadRepository, BookReadRepository>();
        services.AddScoped<IBookWriteRepository, BookWriteRepository>();

        return services;
    }
}
