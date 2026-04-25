using InfraPlayground.Auth.Application.Common.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;

namespace InfraPlayground.Auth.Infrastructure.Authorization;

/// <summary>
/// PROBLEM:
///   Her permission için services.AddAuthorization(o => o.AddPolicy(...))
///   yazmak istemiyoruz. 50 permission = 50 satır boilerplate.
///
/// ÇÖZÜM:
///   Custom policy provider. ASP.NET her [Authorize(Policy = "X")]
///   gördüğünde önce policy provider'a "X policy'sini bana ver" der.
///   Biz de "Permissions." ile başlayanları runtime'da üretiriz.
///
/// NASIL ÇALIŞIR:
///   1. [Authorize(Policy = "Permissions.Books.Create")] görülür.
///   2. ASP.NET → GetPolicyAsync("Permissions.Books.Create") çağırır.
///   3. Biz: "PermissionRequirement('Permissions.Books.Create')" içeren
///      bir AuthorizationPolicy üretip döneriz.
///   4. PermissionHandler bu requirement'ı görüp claim'leri kontrol eder.
///
/// FALLBACK:
///   Permission ile başlamayan policy isimleri (örn: "AdultUser")
///   için DefaultAuthorizationPolicyProvider'a delege ediyoruz —
///   onun normal AddPolicy kayıtlarını okuması lazım.
///
/// CACHE:
///   Bir kere üretilen policy'yi cache'liyoruz. Aksi halde her request'te
///   yeniden allocate edilir.
/// </summary>
public sealed class PermissionPolicyProvider : IAuthorizationPolicyProvider
{
    private const string PermissionPrefix = "Permissions.";

    private readonly DefaultAuthorizationPolicyProvider _fallback;
    private readonly Dictionary<string, AuthorizationPolicy> _cache = new(StringComparer.Ordinal);
    private readonly Lock _cacheLock = new();

    public PermissionPolicyProvider(IOptions<AuthorizationOptions> options)
    {
        // Fallback: AddPolicy ile statik kaydedilen policy'ler (örn: "AdultUser")
        _fallback = new DefaultAuthorizationPolicyProvider(options);
    }

    public Task<AuthorizationPolicy> GetDefaultPolicyAsync() => _fallback.GetDefaultPolicyAsync();

    public Task<AuthorizationPolicy?> GetFallbackPolicyAsync() => _fallback.GetFallbackPolicyAsync();

    public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
    {
        // Permission policy değilse, varsayılan provider'a bırak
        if (!policyName.StartsWith(PermissionPrefix, StringComparison.Ordinal))
            return _fallback.GetPolicyAsync(policyName);

        // Cache'te var mı?
        lock (_cacheLock)
        {
            if (_cache.TryGetValue(policyName, out var cached))
                return Task.FromResult<AuthorizationPolicy?>(cached);
        }

        // Yoksa üret
        var policy = new AuthorizationPolicyBuilder()
            .RequireAuthenticatedUser()                              // Önce login olmalı
            .AddRequirements(new PermissionRequirement(policyName))  // Sonra permission
            .Build();

        lock (_cacheLock)
        {
            _cache[policyName] = policy;
        }

        return Task.FromResult<AuthorizationPolicy?>(policy);
    }
}
