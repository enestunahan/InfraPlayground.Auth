using InfraPlayground.Auth.Domain.Common;

namespace InfraPlayground.Auth.Domain.Entities.Identity;

public sealed class Permission : BaseEntity
{
    public string Code { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ICollection<RolePermission> RolePermissions { get; set; } = [];
}
