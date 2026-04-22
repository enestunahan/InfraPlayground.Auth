using InfraPlayground.Auth.Application.Common.Security;
using MediatR;

namespace InfraPlayground.Auth.Application.Features.Auth.Commands.Login;

public sealed record LoginCommand(string UserNameOrEmail, string Password) : IRequest<TokenDto>;
