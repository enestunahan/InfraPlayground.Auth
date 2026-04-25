using InfraPlayground.Auth.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;

namespace InfraPlayground.Auth.Infrastructure.Authorization;

/// <summary>
/// PermissionRequirement için karar verici.
///
/// JWT içinde "permission" tipinde claim'ler vardır. Bunlar JwtTokenService
/// tarafından login sırasında basıldı. Burada sadece okuyup kıyaslıyoruz —
/// DB'ye gidilmez (stateless authorization).
///
/// Aranan permission claim'leri arasında varsa Succeed; yoksa sessiz dön.
/// </summary>
public sealed class PermissionHandler : AuthorizationHandler<PermissionRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        PermissionRequirement requirement)
    {
        var hasPermission = context.User.Claims.Any(c =>
            c.Type == Permissions.ClaimType &&
            c.Value == requirement.Permission);

        if (hasPermission)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
