using System.Globalization;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;

namespace InfraPlayground.Auth.Infrastructure.Authorization;

/// <summary>
/// MinimumAgeRequirement için karar verici.
///
/// Çalışma mantığı:
///   1. JWT içindeki "birthDate" claim'i okunur.
///   2. Bugünün tarihinden çıkarılarak yaş hesaplanır.
///   3. requirement.MinimumAge'ten büyük/eşitse context.Succeed çağrılır.
///
/// ÇOK ÖNEMLİ:
///   - Succeed çağırmazsan policy başarısız olur (default davranış).
///   - context.Fail() ÇAĞIRMA. Fail çağırırsan diğer handler'ların başarılı
///     sonucu da geçersiz olur — "veto" davranışı. Çoğu zaman istemediğin şey.
///   - Sessizce Task.CompletedTask dön: "ben bu requirement'ı doğrulayamadım,
///     başka bir handler doğrulayabilirse devam etsin."
///
/// LIFECYCLE: Singleton. Handler'lar state tutmaz; aynı instance tüm
/// request'lere hizmet eder. Bu yüzden constructor'da state cache'leme.
/// </summary>
public sealed class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    public const string BirthDateClaimType = "birthDate";

    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        // 1. Claim'i bul
        var birthDateClaim = context.User.FindFirst(BirthDateClaimType);
        if (birthDateClaim is null)
            return Task.CompletedTask; // Claim yok → bu handler karar veremez

        // 2. Parse et (ISO format: "yyyy-MM-dd")
        if (!DateOnly.TryParseExact(
                birthDateClaim.Value,
                "yyyy-MM-dd",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var birthDate))
        {
            return Task.CompletedTask; // Format bozuk → karar verme
        }

        // 3. Yaş hesabı (sadece yıl farkı yetmez; doğum günü gelmediyse 1 eksik)
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - birthDate.Year;
        if (birthDate > today.AddYears(-age))
            age--;

        // 4. Karar
        if (age >= requirement.MinimumAge)
            context.Succeed(requirement);

        return Task.CompletedTask;
    }
}
