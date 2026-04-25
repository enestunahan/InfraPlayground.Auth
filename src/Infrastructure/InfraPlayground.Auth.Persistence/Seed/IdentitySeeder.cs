using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace InfraPlayground.Auth.Persistence.Seed;

public static class IdentitySeeder
{
    public const string DefaultPassword = "Password123!";

    private static readonly string[] Roles = [AppRoles.Admin, AppRoles.Editor, AppRoles.User, AppRoles.Viewer];

    private static readonly SeedUser[] Users =
    [
        new("admin", "admin@infraplayground.local", "Admin User", AppRoles.Admin),
        new("enes.editor", "enes.editor@infraplayground.local", "Enes Editor", AppRoles.Editor),
        new("enes.user", "enes.user@infraplayground.local", "Enes User", AppRoles.User),
        new("enes.viewer", "enes.viewer@infraplayground.local", "Enes Viewer", AppRoles.Viewer)
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("IdentitySeeder");

        foreach (var roleName in Roles)
        {
            if (await roleManager.RoleExistsAsync(roleName))
                continue;

            var roleResult = await roleManager.CreateAsync(new AppRole
            {
                Id = Guid.NewGuid().ToString(),
                Name = roleName
            });

            if (!roleResult.Succeeded)
            {
                var errors = string.Join(" | ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                throw new InvalidOperationException($"Rol oluşturulamadı ({roleName}): {errors}");
            }

            logger.LogInformation("Seed: rol oluşturuldu -> {Role}", roleName);
        }

        foreach (var seed in Users)
        {
            var existing = await userManager.FindByNameAsync(seed.UserName);
            if (existing is null)
            {
                var user = new AppUser
                {
                    Id = Guid.NewGuid().ToString(),
                    UserName = seed.UserName,
                    Email = seed.Email,
                    EmailConfirmed = true,
                    NameSurname = seed.NameSurname
                };

                var createResult = await userManager.CreateAsync(user, DefaultPassword);
                if (!createResult.Succeeded)
                {
                    var errors = string.Join(" | ", createResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Kullanıcı oluşturulamadı ({seed.UserName}): {errors}");
                }

                var roleAssign = await userManager.AddToRoleAsync(user, seed.Role);
                if (!roleAssign.Succeeded)
                {
                    var errors = string.Join(" | ", roleAssign.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Rol ataması başarısız ({seed.UserName} -> {seed.Role}): {errors}");
                }

                logger.LogInformation("Seed: kullanıcı oluşturuldu -> {UserName} ({Role})", seed.UserName, seed.Role);
            }
            else if (!await userManager.IsInRoleAsync(existing, seed.Role))
            {
                var roleAssign = await userManager.AddToRoleAsync(existing, seed.Role);
                if (!roleAssign.Succeeded)
                {
                    var errors = string.Join(" | ", roleAssign.Errors.Select(e => $"{e.Code}: {e.Description}"));
                    throw new InvalidOperationException($"Rol ataması başarısız ({seed.UserName} -> {seed.Role}): {errors}");
                }
            }
        }
    }

    private sealed record SeedUser(string UserName, string Email, string NameSurname, string Role);
}
