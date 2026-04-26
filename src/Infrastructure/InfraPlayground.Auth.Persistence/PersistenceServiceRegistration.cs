using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Application.Common.Repositories.Books;
using InfraPlayground.Auth.Domain.Entities.Identity;
using InfraPlayground.Auth.Persistence.Contexts;
using InfraPlayground.Auth.Persistence.Repositories;
using InfraPlayground.Auth.Persistence.Repositories.Books;
using InfraPlayground.Auth.Persistence.Security;
using Microsoft.AspNetCore.Identity;
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

        services.AddIdentityCore<AppUser>(options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequiredLength = 6;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireDigit = false;

                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
                options.Lockout.MaxFailedAccessAttempts = 5;
            })
            .AddRoles<AppRole>()
            .AddEntityFrameworkStores<InfraPlaygroundAuthDbContext>();

        services.AddScoped(typeof(IReadRepository<>), typeof(ReadRepository<>));
        services.AddScoped(typeof(IWriteRepository<>), typeof(WriteRepository<>));
        services.AddScoped<IBookReadRepository, BookReadRepository>();
        services.AddScoped<IBookWriteRepository, BookWriteRepository>();
        services.AddScoped<IPermissionLookupService, PermissionLookupService>();

        return services;
    }
}
