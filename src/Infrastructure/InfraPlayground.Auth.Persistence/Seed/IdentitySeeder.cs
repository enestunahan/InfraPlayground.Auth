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

    // BirthDate'leri policy testleri için bilinçli farklı yaşlarda seçtik:
    //  - admin     -> 30 yaş  (her şeye yetkili, yaşı da büyük)
    //  - editor    -> 25 yaş
    //  - user      -> 17 yaş  (MinimumAge(18) policy'sini test etmek için bilinçli küçük!)
    //  - viewer    -> 22 yaş
    private static readonly SeedUser[] Users =
    [
        new("admin",       "admin@infraplayground.local",       "Admin User",  AppRoles.Admin,  new DateOnly(1995, 1, 1)),
        new("enes.editor", "enes.editor@infraplayground.local", "Enes Editor", AppRoles.Editor, new DateOnly(2000, 6, 15)),
        new("enes.user",   "enes.user@infraplayground.local",   "Enes User",   AppRoles.User,   new DateOnly(2008, 9, 1)),
        new("enes.viewer", "enes.viewer@infraplayground.local", "Enes Viewer", AppRoles.Viewer, new DateOnly(2003, 3, 20))
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
                    NameSurname = seed.NameSurname,
                    BirthDate = seed.BirthDate
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
            else
            {
                // Kullanıcı zaten var. Şu mantığı izliyoruz:
                //  - Eksik rol varsa ekle.
                //  - BirthDate hâlâ null ise (eski schema'dan kalma) seed değeriyle doldur.
                //  - Mevcut bir BirthDate'e ASLA dokunma — kullanıcı kendi profilinden değiştirmiş olabilir.
                //
                // Bu pattern "idempotent backfill" — seeder her açılışta güvenle çalışır,
                // veriyi bozmaz, sadece eksiği tamamlar.
                var needsUpdate = false;

                if (existing.BirthDate is null)
                {
                    existing.BirthDate = seed.BirthDate;
                    needsUpdate = true;
                    logger.LogInformation("Seed: BirthDate dolduruldu -> {UserName} ({BirthDate})",
                        seed.UserName, seed.BirthDate);
                }

                if (!await userManager.IsInRoleAsync(existing, seed.Role))
                {
                    var roleAssign = await userManager.AddToRoleAsync(existing, seed.Role);
                    if (!roleAssign.Succeeded)
                    {
                        var errors = string.Join(" | ", roleAssign.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Rol ataması başarısız ({seed.UserName} -> {seed.Role}): {errors}");
                    }
                }

                if (needsUpdate)
                {
                    var updateResult = await userManager.UpdateAsync(existing);
                    if (!updateResult.Succeeded)
                    {
                        var errors = string.Join(" | ", updateResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
                        throw new InvalidOperationException($"Kullanıcı güncellenemedi ({seed.UserName}): {errors}");
                    }
                }
            }
        }
    }

    private sealed record SeedUser(string UserName, string Email, string NameSurname, string Role, DateOnly BirthDate);
}
