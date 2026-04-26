using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Domain.Entities.Identity;
using InfraPlayground.Auth.Persistence.Contexts;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;

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

    private static readonly SeedPermission[] PermissionSeeds =
    [
        new(Permissions.Books.Create, "Kitap kaydı oluşturma izni"),
        new(Permissions.Books.Read, "Kitap kayıtlarını görüntüleme izni"),
        new(Permissions.Books.Update, "Kitap kayıtlarını güncelleme izni"),
        new(Permissions.Books.Delete, "Kitap kayıtlarını silme izni"),

        new(Permissions.PermissionManagement.Create, "Permission kaydı oluşturma izni"),
        new(Permissions.PermissionManagement.Read, "Permission kayıtlarını görüntüleme izni"),
        new(Permissions.PermissionManagement.Update, "Permission kayıtlarını güncelleme izni"),
        new(Permissions.PermissionManagement.Delete, "Permission kayıtlarını silme izni")
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<AppRole>>();
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var dbContext = services.GetRequiredService<InfraPlaygroundAuthDbContext>();
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

        await SeedPermissionsAsync(dbContext, roleManager, logger);

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

    private static async Task SeedPermissionsAsync(
        InfraPlaygroundAuthDbContext dbContext,
        RoleManager<AppRole> roleManager,
        ILogger logger)
    {
        var existingPermissionCodes = await dbContext.Permissions
            .AsNoTracking()
            .Select(permission => permission.Code)
            .ToListAsync();

        var existingPermissionCodeSet = existingPermissionCodes.ToHashSet(StringComparer.Ordinal);
        var missingPermissions = PermissionSeeds
            .Where(permissionSeed => !existingPermissionCodeSet.Contains(permissionSeed.Code))
            .Select(permissionSeed => new Permission
            {
                Id = Guid.NewGuid(),
                Code = permissionSeed.Code,
                Description = permissionSeed.Description
            })
            .ToList();

        if (missingPermissions.Count > 0)
        {
            await dbContext.Permissions.AddRangeAsync(missingPermissions);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seed: {PermissionCount} permission eklendi.", missingPermissions.Count);
        }
        var newPermissionCodeSet = missingPermissions
            .Select(permission => permission.Code)
            .ToHashSet(StringComparer.Ordinal);

        var roleList = await roleManager.Roles
            .Where(role => role.Name != null)
            .Select(role => new { role.Name, role.Id })
            .ToListAsync();
        var roleIdByName = roleList.ToDictionary(role => role.Name!, role => role.Id, StringComparer.Ordinal);

        var permissionList = await dbContext.Permissions
            .AsNoTracking()
            .Select(permission => new { permission.Code, permission.Id })
            .ToListAsync();
        var permissionIdByCode = permissionList.ToDictionary(permission => permission.Code, permission => permission.Id, StringComparer.Ordinal);

        var existingAssignments = await dbContext.RolePermissions
            .AsNoTracking()
            .Select(rolePermission => new { rolePermission.RoleId, rolePermission.PermissionId })
            .ToListAsync();

        var seedAllAssignments = existingAssignments.Count == 0;

        // Role-permission tablosu boşsa ilk bootstrap'ta tüm default atamaları basarız.
        // Tablo doluysa yalnızca bu çalışmada yeni eklenen permission kodlarının
        // default atamalarını ekleriz; mevcut manuel değişikliklere dokunmayız.
        if (!seedAllAssignments && newPermissionCodeSet.Count == 0)
            return;

        var existingAssignmentKeys = existingAssignments
            .Select(assignment => $"{assignment.RoleId}:{assignment.PermissionId}")
            .ToHashSet(StringComparer.Ordinal);

        var missingAssignments = new List<RolePermission>();

        foreach (var (roleName, permissionCodes) in RolePermissions.Map)
        {
            if (!roleIdByName.TryGetValue(roleName, out var roleId))
                continue;

            foreach (var permissionCode in permissionCodes.Distinct(StringComparer.Ordinal))
            {
                if (!seedAllAssignments && !newPermissionCodeSet.Contains(permissionCode))
                    continue;

                if (!permissionIdByCode.TryGetValue(permissionCode, out var permissionId))
                    continue;

                var assignmentKey = $"{roleId}:{permissionId}";
                if (existingAssignmentKeys.Contains(assignmentKey))
                    continue;

                missingAssignments.Add(new RolePermission
                {
                    RoleId = roleId,
                    PermissionId = permissionId
                });
                existingAssignmentKeys.Add(assignmentKey);
            }
        }

        if (missingAssignments.Count > 0)
        {
            await dbContext.RolePermissions.AddRangeAsync(missingAssignments);
            await dbContext.SaveChangesAsync();
            logger.LogInformation("Seed: {AssignmentCount} role-permission eşleştirmesi eklendi.", missingAssignments.Count);
        }
    }

    private sealed record SeedUser(string UserName, string Email, string NameSurname, string Role, DateOnly BirthDate);
    private sealed record SeedPermission(string Code, string Description);
}
