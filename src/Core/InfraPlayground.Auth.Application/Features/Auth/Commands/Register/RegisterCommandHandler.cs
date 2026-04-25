using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Common.Exceptions;
using InfraPlayground.Auth.Domain.Entities.Identity;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace InfraPlayground.Auth.Application.Features.Auth.Commands.Register;

public sealed class RegisterCommandHandler(UserManager<AppUser> userManager)
    : IRequestHandler<RegisterCommand, RegisterResponse>
{
    public async Task<RegisterResponse> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        if (request.Password != request.PasswordConfirm)
            throw new BusinessException("Şifreler birbiriyle eşleşmiyor.");

        var user = new AppUser
        {
            Id = Guid.NewGuid().ToString(),
            UserName = request.UserName,
            Email = request.Email,
            NameSurname = request.NameSurname,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" | ", result.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new BusinessException($"Kullanıcı oluşturulamadı. {errors}");
        }

        var roleResult = await userManager.AddToRoleAsync(user, AppRoles.User);
        if (!roleResult.Succeeded)
        {
            var errors = string.Join(" | ", roleResult.Errors.Select(e => $"{e.Code}: {e.Description}"));
            throw new BusinessException($"Kullanıcı rolü atanamadı. {errors}");
        }

        return new RegisterResponse(user.Id, user.UserName!, user.Email!);
    }
}
