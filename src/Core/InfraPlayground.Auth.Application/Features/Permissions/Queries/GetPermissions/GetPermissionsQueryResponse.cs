namespace InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissions;

public sealed record PermissionListItemDto(
    Guid Id,
    string Code,
    string Description,
    int AssignedRoleCount);

public sealed class GetPermissionsQueryResponse
{
    public IReadOnlyList<PermissionListItemDto> Permissions { get; init; } = [];
}
