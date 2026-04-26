using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissionById;

public sealed class GetPermissionByIdQueryHandler(IReadRepository<Permission> permissionReadRepository)
    : IRequestHandler<GetPermissionByIdQuery, PermissionDetailDto?>
{
    public async Task<PermissionDetailDto?> Handle(
        GetPermissionByIdQuery request,
        CancellationToken cancellationToken)
    {
        var permission = await permissionReadRepository.GetAll(false)
            .Where(item => item.Id == request.Id)
            .Select(item => new
            {
                item.Id,
                item.Code,
                item.Description,
                Roles = item.RolePermissions
                    .Select(rolePermission => rolePermission.Role.Name)
                    .Where(roleName => roleName != null)
                    .Select(roleName => roleName!)
                    .OrderBy(roleName => roleName)
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (permission is null)
            return null;

        return new PermissionDetailDto(
            permission.Id,
            permission.Code,
            permission.Description,
            permission.Roles);
    }
}
