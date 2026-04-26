using MediatR;

namespace InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissions;

public sealed record GetPermissionsQuery : IRequest<GetPermissionsQueryResponse>;
