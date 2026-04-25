using InfraPlayground.Auth.Application.Common.Security;
using MediatR;

namespace InfraPlayground.Auth.Application.Features.Auth.Commands.RefreshTokenLogin;

public sealed record RefreshTokenLoginCommand(string RefreshToken) : IRequest<TokenDto>;
