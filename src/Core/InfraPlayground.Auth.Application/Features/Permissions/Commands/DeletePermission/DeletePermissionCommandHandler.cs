using InfraPlayground.Auth.Application.Common.Exceptions;
using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Entities.Identity;
using MediatR;

namespace InfraPlayground.Auth.Application.Features.Permissions.Commands.DeletePermission;

public sealed class DeletePermissionCommandHandler(IWriteRepository<Permission> permissionWriteRepository)
    : IRequestHandler<DeletePermissionCommand, Unit>
{
    public async Task<Unit> Handle(DeletePermissionCommand request, CancellationToken cancellationToken)
    {
        var removed = await permissionWriteRepository.RemoveAsync(request.Id.ToString(), cancellationToken);
        if (!removed)
            throw new NotFoundException("Permission bulunamadı.");

        await permissionWriteRepository.SaveAsync(cancellationToken);
        return Unit.Value;
    }
}
