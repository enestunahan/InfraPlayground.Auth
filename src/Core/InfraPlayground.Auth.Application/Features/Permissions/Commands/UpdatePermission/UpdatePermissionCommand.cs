using MediatR;

namespace InfraPlayground.Auth.Application.Features.Permissions.Commands.UpdatePermission;

public sealed record UpdatePermissionCommand(
    Guid Id,
    string Code,
    string Description) : IRequest<Unit>;
