using Microsoft.AspNetCore.Identity;

namespace InfraPlayground.Auth.Domain.Entities.Identity;

public class AppRole : IdentityRole<string>
{
    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
