using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Common.Security;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace InfraPlayground.Auth.Infrastructure.Authorization;

/// <summary>
/// Authorization ile ilgili tüm DI kayıtlarını tek bir yerde topluyoruz.
/// API'nin Program.cs'i bu method'u çağırıyor.
///
/// İçeriği:
///   1. Statik policy'ler (örn: "AdultUser") — AddAuthorization içindeki AddPolicy'ler.
///   2. Custom requirement handler'ları (Singleton kayıt — handler'lar stateless).
///   3. PermissionPolicyProvider — dinamik permission policy'leri için.
/// </summary>
public static class AuthorizationServiceRegistration
{
    public static IServiceCollection AddAppAuthorization(this IServiceCollection services)
    {
        services.AddAuthorization(options =>
        {
            // ---- Statik (önceden bilinen) policy'ler ----
            //
            // "AdultUser" policy'si: User rolüne sahip + 18 yaşından büyük.
            // Birden fazla requirement AND'lenir; ikisi de geçmesi gerekir.
            options.AddPolicy(Policies.AdultUser, policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AppRoles.User);
                policy.AddRequirements(new MinimumAgeRequirement(18));
            });

            // Buraya yeni statik policy'ler eklenebilir. Permission policy'leri
            // için bir şey yazmamıza gerek yok — onları PermissionPolicyProvider üretiyor.
        });

        // ---- Handler kayıtları ----
        //
        // Handler'lar stateless olduğu için Singleton. Birden fazla handler
        // aynı requirement'a hizmet edebilir; biri Succeed çağırırsa yeter (OR).
        services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
        services.AddSingleton<IAuthorizationHandler, PermissionHandler>();

        // ---- Custom policy provider ----
        //
        // Dinamik permission policy'leri için. Yerine geçtiği için Singleton.
        services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();

        return services;
    }
}
