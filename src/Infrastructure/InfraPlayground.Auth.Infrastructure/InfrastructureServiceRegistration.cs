using InfraPlayground.Auth.Application.Common.Security;
using InfraPlayground.Auth.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InfraPlayground.Auth.Infrastructure;

public static class InfrastructureServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtTokenOptions>(configuration.GetSection(JwtTokenOptions.SectionName));

        services.AddScoped<ITokenService, JwtTokenService>();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        return services;
    }
}
