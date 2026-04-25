using Microsoft.AspNetCore.Authorization;

namespace InfraPlayground.Auth.Infrastructure.Authorization;

/// <summary>
/// "Kullanıcının şu permission'a sahip olması lazım" ihtiyacı.
///
/// Tek bir requirement sınıfıyla tüm permission'ları temsil edebiliyoruz —
/// her permission için ayrı sınıf yazmaya gerek yok. Hangi permission
/// olduğunu Permission property'si tutar.
/// </summary>
public sealed class PermissionRequirement(string permission) : IAuthorizationRequirement
{
    public string Permission { get; } = permission;
}
