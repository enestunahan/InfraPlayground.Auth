using MediatR;

namespace InfraPlayground.Auth.Application.Features.Permissions.Commands.CreatePermission;

public sealed record CreatePermissionCommand(
    string Code,
    string Description) : IRequest<CreatePermissionCommandResponse>;

public sealed record CreatePermissionCommandResponse(
    Guid Id,
    string Code,
    string Description);
