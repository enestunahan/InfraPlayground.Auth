namespace InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissionById;

public sealed record PermissionDetailDto(
    Guid Id,
    string Code,
    string Description,
    IReadOnlyList<string> Roles);
