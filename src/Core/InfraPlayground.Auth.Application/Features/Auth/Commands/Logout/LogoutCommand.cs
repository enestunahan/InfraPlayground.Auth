using MediatR;

namespace InfraPlayground.Auth.Application.Features.Auth.Commands.Logout;

public sealed record LogoutCommand(string UserId) : IRequest<Unit>;
