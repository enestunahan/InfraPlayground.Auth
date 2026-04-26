namespace InfraPlayground.Auth.Application.Common.Authorization;

public interface IPermissionLookupService
{
    Task<IReadOnlyCollection<string>> GetPermissionsForRolesAsync(
        IEnumerable<string> roleNames,
        CancellationToken cancellationToken = default);
}
