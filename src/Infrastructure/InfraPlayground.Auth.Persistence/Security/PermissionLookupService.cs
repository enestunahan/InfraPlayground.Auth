using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Persistence.Security;

public sealed class PermissionLookupService(InfraPlaygroundAuthDbContext dbContext) : IPermissionLookupService
{
    public async Task<IReadOnlyCollection<string>> GetPermissionsForRolesAsync(
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default)
    {
        var normalizedRoleNames = roleNames
            .Where(roleName => !string.IsNullOrWhiteSpace(roleName))
            .Select(roleName => roleName.Trim().ToUpperInvariant())
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (normalizedRoleNames.Length == 0)
            return Array.Empty<string>();

        var permissions = await dbContext.RolePermissions
            .AsNoTracking()
            .Where(rolePermission =>
                rolePermission.Role.NormalizedName != null &&
                normalizedRoleNames.Contains(rolePermission.Role.NormalizedName))
            .Select(rolePermission => rolePermission.Permission.Code)
            .Distinct()
            .OrderBy(permissionCode => permissionCode)
            .ToListAsync(cancellationToken);

        return permissions;
    }
}
