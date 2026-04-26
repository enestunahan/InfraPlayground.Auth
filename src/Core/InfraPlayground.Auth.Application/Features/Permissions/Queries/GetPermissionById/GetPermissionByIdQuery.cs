using MediatR;

namespace InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissionById;

public sealed record GetPermissionByIdQuery(Guid Id) : IRequest<PermissionDetailDto?>;
