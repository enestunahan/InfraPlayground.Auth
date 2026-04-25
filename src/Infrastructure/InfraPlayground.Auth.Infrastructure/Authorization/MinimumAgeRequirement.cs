using Microsoft.AspNetCore.Authorization;

namespace InfraPlayground.Auth.Infrastructure.Authorization;

/// <summary>
/// "Bir kullanıcının en az şu yaşta olması gerekir" ihtiyacını ifade eder.
///
/// REQUIREMENT vs HANDLER:
///   - Requirement = sadece DATA. (Burada: minimum yaş kaç?)
///   - Handler     = MANTIK.      (Yaşı hesaplama, kıyaslama)
///
/// Requirement'ın IAuthorizationRequirement'ı implement etmesi şart;
/// boş bir marker interface'tir, framework ile entegrasyon için kullanılır.
/// İçinde başka method/logic OLMAMALIDIR — saf veri taşır.
/// </summary>
public sealed class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}
