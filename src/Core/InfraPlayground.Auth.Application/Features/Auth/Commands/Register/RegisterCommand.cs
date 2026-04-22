using MediatR;

namespace InfraPlayground.Auth.Application.Features.Auth.Commands.Register;

public sealed record RegisterCommand(
    string NameSurname,
    string UserName,
    string Email,
    string Password,
    string PasswordConfirm) : IRequest<RegisterResponse>;

public sealed record RegisterResponse(string UserId, string UserName, string Email);
