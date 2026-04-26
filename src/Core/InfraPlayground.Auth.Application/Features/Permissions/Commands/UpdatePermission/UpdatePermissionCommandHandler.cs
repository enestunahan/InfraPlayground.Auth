using InfraPlayground.Auth.Application.Common.Exceptions;
using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Entities.Identity;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Application.Features.Permissions.Commands.UpdatePermission;

public sealed class UpdatePermissionCommandHandler(
    IReadRepository<Permission> permissionReadRepository,
    IWriteRepository<Permission> permissionWriteRepository)
    : IRequestHandler<UpdatePermissionCommand, Unit>
{
    public async Task<Unit> Handle(UpdatePermissionCommand request, CancellationToken cancellationToken)
    {
        var permission = await permissionReadRepository.GetByIdAsync(
            request.Id.ToString(),
            tracking: true,
            cancellationToken: cancellationToken);

        if (permission is null)
            throw new NotFoundException("Permission bulunamadı.");

        var code = request.Code?.Trim() ?? string.Empty;
        var description = request.Description?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(code))
            throw new BusinessException("Permission code boş olamaz.");

        if (!code.StartsWith("Permissions.", StringComparison.Ordinal))
            throw new BusinessException("Permission code 'Permissions.' prefix'i ile başlamalı.");

        if (string.IsNullOrWhiteSpace(description))
            throw new BusinessException("Permission description boş olamaz.");

        var codeUpper = code.ToUpperInvariant();
        var duplicateExists = await permissionReadRepository.GetAll(false)
            .AnyAsync(
                item => item.Id != request.Id && item.Code.ToUpper() == codeUpper,
                cancellationToken);

        if (duplicateExists)
            throw new BusinessException($"'{code}' permission kodu zaten mevcut.");

        permission.Code = code;
        permission.Description = description;

        permissionWriteRepository.Update(permission);
        await permissionWriteRepository.SaveAsync(cancellationToken);

        return Unit.Value;
    }
}
