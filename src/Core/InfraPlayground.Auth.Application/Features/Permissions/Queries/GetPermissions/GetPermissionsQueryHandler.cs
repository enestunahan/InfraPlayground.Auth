using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissions;

public sealed class GetPermissionsQueryHandler(IReadRepository<Permission> permissionReadRepository)
    : IRequestHandler<GetPermissionsQuery, GetPermissionsQueryResponse>
{
    public async Task<GetPermissionsQueryResponse> Handle(
        GetPermissionsQuery request,
        CancellationToken cancellationToken)
    {
        var permissions = await permissionReadRepository.GetAll(false)
            .OrderBy(permission => permission.Code)
            .Select(permission => new PermissionListItemDto(
                permission.Id,
                permission.Code,
                permission.Description,
                permission.RolePermissions.Count))
            .ToListAsync(cancellationToken);

        return new GetPermissionsQueryResponse
        {
            Permissions = permissions
        };
    }
}
