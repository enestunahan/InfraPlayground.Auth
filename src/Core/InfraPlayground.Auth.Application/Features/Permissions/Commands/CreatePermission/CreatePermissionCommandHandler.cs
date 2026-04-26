using InfraPlayground.Auth.Application.Common.Exceptions;
using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Application.Features.Permissions.Commands.CreatePermission;

public sealed class CreatePermissionCommandHandler(
    IReadRepository<Permission> permissionReadRepository,
    IWriteRepository<Permission> permissionWriteRepository)
    : IRequestHandler<CreatePermissionCommand, CreatePermissionCommandResponse>
{
    public async Task<CreatePermissionCommandResponse> Handle(
        CreatePermissionCommand request,
        CancellationToken cancellationToken)
    {
        var code = request.Code?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessException("Permission code boş olamaz.");

        if (!code.StartsWith("Permissions.", StringComparison.Ordinal))
            throw new BusinessException("Permission code 'Permissions.' prefix'i ile başlamalı.");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("Permission description boş olamaz.");

        var codeUpper = code.ToUpperInvariant();
        var exists = await permissionReadRepository.GetAll(false)
            .AnyAsync(permission => permission.Code.ToUpper() == codeUpper, cancellationToken);

        if (exists)
            throw new BusinessException($"'{code}' permission kodu zaten mevcut.");

        var permission = new Permission
        {
            Id = Guid.NewGuid(),
            Code = code,
            Description = description
        };

        await permissionWriteRepository.AddAsync(permission, cancellationToken);
        await permissionWriteRepository.SaveAsync(cancellationToken);

        return new CreatePermissionCommandResponse(permission.Id, permission.Code, permission.Description);
    }
}
