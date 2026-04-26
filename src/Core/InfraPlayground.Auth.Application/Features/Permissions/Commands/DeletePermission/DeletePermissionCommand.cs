using MediatR;

namespace InfraPlayground.Auth.Application.Features.Permissions.Commands.DeletePermission;

public sealed record DeletePermissionCommand(Guid Id) : IRequest<Unit>;
