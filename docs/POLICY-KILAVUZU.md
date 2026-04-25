# Policy & Permission Kılavuzu

Bu doküman role-based authorization'dan **policy-based** ve **permission-based** authorization'a geçişi anlatır. AUTH-KILAVUZU.md'yi okuduktan sonra okumalısın.

---

## 1. Neden Policy?

Önce role-based başladık:
```csharp
[Authorize(Roles = "Admin,Editor")]
```

Sınırları:
- "Admin VE 18+ yaş" gibi **AND** mantığı kolay yazılamıyor.
- "Yaşı 18'den büyük" gibi rol olmayan kuralları ifade edemiyorsun.
- Endpoint sayısı arttıkça rol stringleri kontrolden çıkıyor.

**Policy** = "yetkilendirme kurallarımı tek isim altında topluyorum" demek. İçinde rol kontrolü, claim kontrolü, custom kural — hepsi olabilir.

---

## 2. Üçlü Anatomi: Requirement, Handler, Policy

### Requirement — DATA
```csharp
public sealed class MinimumAgeRequirement(int minimumAge) : IAuthorizationRequirement
{
    public int MinimumAge { get; } = minimumAge;
}
```
- Sadece veri taşır. Mantık yok.
- `IAuthorizationRequirement` boş bir marker interface — framework bu tipi tanısın diye.

### Handler — MANTIK
```csharp
public sealed class MinimumAgeHandler : AuthorizationHandler<MinimumAgeRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        MinimumAgeRequirement requirement)
    {
        // 1. claim oku  2. yaş hesapla  3. requirement.MinimumAge ile kıyasla
        if (yas >= requirement.MinimumAge)
            context.Succeed(requirement);
        return Task.CompletedTask;
    }
}
```
- `Succeed` çağırırsa: bu requirement geçti.
- Hiçbir şey yapmazsa: bu handler karar vermedi. Başka handler varsa ona şans verir.
- `Fail()` çağırırsa: bütün policy'yi öldürür (veto). **Çağırma.**

### Policy — KOMPOZİSYON
```csharp
options.AddPolicy("AdultUser", policy =>
{
    policy.RequireRole("User");                              // 1. requirement
    policy.AddRequirements(new MinimumAgeRequirement(18));   // 2. requirement
});
```
- Bir policy içindeki tüm requirement'lar **AND**'lenir.
- Her requirement için en az bir handler `Succeed` demeli.

---

## 3. AND mı OR mu?

| Senaryo | Davranış |
|---|---|
| Aynı **policy** içinde 2 requirement | AND (ikisi de geçmeli) |
| Aynı **requirement** için 2 handler | OR (biri Succeed dese yeter) |
| `RequireRole("A,B")` | OR (A veya B) |
| 2 ayrı `[Authorize]` attribute | AND (ikisi de geçmeli) |

---

## 4. Bu Projedeki Statik Policy: "AdultUser"

[AuthorizationServiceRegistration.cs](../src/Infrastructure/InfraPlayground.Auth.Infrastructure/Authorization/AuthorizationServiceRegistration.cs):
```csharp
options.AddPolicy(Policies.AdultUser, policy =>
{
    policy.RequireAuthenticatedUser();
    policy.RequireRole(AppRoles.User);
    policy.AddRequirements(new MinimumAgeRequirement(18));
});
```

Endpoint kullanımı:
```csharp
[HttpGet("adult-user")]
[Authorize(Policy = Policies.AdultUser)]
public IActionResult AdultUser() { ... }
```

### BirthDate akışı
1. `AppUser.BirthDate` (DateOnly?) — Domain entity'sinde tutulur.
2. Login olunca `JwtTokenService` → JWT'ye `birthDate` claim'i basılır (yyyy-MM-dd).
3. Endpoint çağrılınca `MinimumAgeHandler` claim'i okur, yaşı hesaplar.

### Test akışı
Seed kullanıcılarının yaşları bilinçli olarak farklı:
| Kullanıcı | Rol | Yaş | `/adult-user` Sonuç |
|---|---|---|---|
| admin | Admin | 30 | ❌ 403 (User rolü yok) |
| enes.editor | Editor | 25 | ❌ 403 (User rolü yok) |
| **enes.user** | **User** | **17** | **❌ 403 (yaş yetmiyor)** |
| enes.viewer | Viewer | 22 | ❌ 403 (User rolü yok) |

Yani şu anda hiç kimse geçemez. Test için register endpoint'inden 18+ User rolüyle bir kullanıcı oluşturup test edebilirsin.

---

## 5. Permission-Based Authorization

### Sorun
Role-based authorization endpoint'lerin rol isimlerini bilmesini gerektirir:
```csharp
[Authorize(Roles = "Admin,Editor")]  // endpoint Admin ve Editor'ı biliyor
```

Yarın "Manager" rolü gelse, **bütün endpoint'leri tarayıp** rol listelerini güncellemek gerekir. Sürdürülemez.

### Çözüm
Endpoint sadece "ne yapmak gerekiyor"u söylesin (`Books.Create`); kimin yapabileceği başka bir yerde tanımlansın.

```csharp
// Endpoint:
[Authorize(Policy = Permissions.Books.Create)]

// Mapping (RolePermissions.cs):
[AppRoles.Admin]  → [Books.Create, Read, Update, Delete]
[AppRoles.Editor] → [Books.Create, Read, Update]
[AppRoles.User]   → [Books.Read]
```

Yarın Manager gelse, sadece haritaya bir satır eklersin. Endpoint'lere dokunmazsın.

### Permission'lar nereye yazılıyor?
JWT içine. Login olduğunda `JwtTokenService`:
```csharp
foreach (var permission in RolePermissions.GetPermissionsForRoles(roles))
    claims.Add(new Claim("permission", permission));
```

JWT payload'unda şöyle görünür:
```json
{
  "role": ["Admin"],
  "permission": [
    "Permissions.Books.Create",
    "Permissions.Books.Read",
    "Permissions.Books.Update",
    "Permissions.Books.Delete"
  ]
}
```

### PermissionHandler
[PermissionHandler.cs](../src/Infrastructure/InfraPlayground.Auth.Infrastructure/Authorization/PermissionHandler.cs):
```csharp
var hasPermission = context.User.Claims.Any(c =>
    c.Type == "permission" && c.Value == requirement.Permission);

if (hasPermission) context.Succeed(requirement);
```
DB'ye gitmiyor — token'daki claim'lere bakıyor. Stateless.

---

## 6. PermissionPolicyProvider — Sihirli Kısım

### Problem
50 permission için 50 satır `AddPolicy` yazmak istemezsin:
```csharp
options.AddPolicy("Permissions.Books.Create", p => p.AddRequirements(new PermissionRequirement("Permissions.Books.Create")));
options.AddPolicy("Permissions.Books.Read", p => p.AddRequirements(new PermissionRequirement("Permissions.Books.Read")));
// ... 48 satır daha
```

### Çözüm: Custom Policy Provider
ASP.NET her `[Authorize(Policy = "X")]` gördüğünde önce policy provider'a sorar: "X policy'sini bana ver." Biz custom provider yazıp "X 'Permissions.' ile başlıyorsa runtime'da üret" diyoruz.

[PermissionPolicyProvider.cs](../src/Infrastructure/InfraPlayground.Auth.Infrastructure/Authorization/PermissionPolicyProvider.cs):
```csharp
public Task<AuthorizationPolicy?> GetPolicyAsync(string policyName)
{
    if (!policyName.StartsWith("Permissions."))
        return _fallback.GetPolicyAsync(policyName);  // statik policy'lere bırak

    var policy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .AddRequirements(new PermissionRequirement(policyName))
        .Build();
    return Task.FromResult<AuthorizationPolicy?>(policy);
}
```

`Permissions.Books.Create` için runtime'da bir policy üretiyor. Önemli: `_fallback` ile DefaultProvider'a delege etmemiz gerekiyor — yoksa "AdultUser" gibi statik policy'ler bulunamaz.

---

## 7. Akış (Permission örneği)

```
HTTP isteği gelir
   ├─ Header: Authorization: Bearer eyJ...
   ↓
[Authentication middleware]
   ├─ JWT imzasını doğrular
   ├─ Token içindeki claim'leri context.User'a yükler
   ├─    "permission": ["Permissions.Books.Create", ...]
   ↓
[Authorization middleware]
   ├─ Endpoint'te [Authorize(Policy = "Permissions.Books.Create")] var
   ├─ PermissionPolicyProvider.GetPolicyAsync("Permissions.Books.Create")
   │     → PermissionRequirement içeren policy üretir
   ├─ PermissionHandler.HandleRequirementAsync çalışır
   │     → context.User.Claims.Any(c => c.Type == "permission" && c.Value == "Permissions.Books.Create")
   │     → varsa Succeed
   ↓
   ├─ Hepsi succeed → endpoint çalışır
   └─ Biri başarısız → 403 Forbidden
```

---

## 8. DI Kayıtları — Lifecycle

[AuthorizationServiceRegistration.cs](../src/Infrastructure/InfraPlayground.Auth.Infrastructure/Authorization/AuthorizationServiceRegistration.cs):

```csharp
services.AddSingleton<IAuthorizationHandler, MinimumAgeHandler>();
services.AddSingleton<IAuthorizationHandler, PermissionHandler>();
services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
```

**Hepsi Singleton, neden?**
- Handler'lar **stateless** — her request'te aynı instance kullanılabilir.
- Policy provider cache tutuyor; Singleton olmasa cache her request yeniden başlardı.
- Eğer handler içinde scoped service'e ihtiyaç olursa (örn: DbContext), `IServiceScopeFactory` inject edip kendi scope'unu açabilirsin. Singleton'a Scoped doğrudan inject edemezsin (captive dependency).

---

## 9. Permission'ı Nasıl Test Ederim?

```bash
# 1. Login (admin)
curl -X POST http://localhost:5106/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmail":"admin","password":"Password123!"}'

# Dönen token'ı al, jwt.io'da decode et — payload'unda permission claim'lerini göreceksin.

# 2. Permission gerektiren endpoint
curl http://localhost:5106/api/home/books-permission-test \
  -H "Authorization: Bearer eyJ..."

# 3. Admin Books CRUD — her birinde farklı permission isteniyor
curl -X DELETE http://localhost:5106/api/admin/books/<id> \
  -H "Authorization: Bearer eyJ..."  # Sadece admin'in bu permission'ı var
```

---

## 10. Role vs Permission — Pratik Kural

| Durum | Tercih |
|---|---|
| Küçük proje, 2-3 rol, sabit | Role-based |
| Yetki sayısı çoğalıyor, esneklik gerek | **Permission-based** |
| Yetki yönetimi UI'dan yapılacak | DB-tabanlı permission (sonraki aşama) |
| Birden fazla kuralın **AND**'i (rol + custom) | Policy |
| Resource-specific (sadece kendi kaydını silebilir) | Resource-based authorization |

---

## 11. Bir Sonraki Aşama: DB Tabanlı Permission

Şu an `RolePermissions.Map` kod içinde sabit. Sonraki adım:
1. `Permission` entity (Code, Description)
2. `RolePermission` join tablosu (RoleId, PermissionId)
3. Login'de DB'den çek, JWT'ye claim olarak bas
4. Admin paneli ile dinamik atama

Bu da bittiğinde:
- Endpoint scan ile `Permission` tablosunu doldurmak (reflection)
- Permission gruplama (modül bazlı)
- Permission cache'leme (her login DB'ye gitmesin)

İlerideki konular knk.

---

## Hızlı Hatırlatma — Neyi Nereye Koyduk?

| Dosya | Katman | İçerik |
|---|---|---|
| `Policies.cs` | Application | Statik policy isim sabitleri |
| `Permissions.cs` | Application | Permission string sabitleri |
| `RolePermissions.cs` | Application | Rol → permission haritası |
| `MinimumAgeRequirement.cs` | Infrastructure | DATA: minimum yaş |
| `MinimumAgeHandler.cs` | Infrastructure | LOGIC: yaş hesabı |
| `PermissionRequirement.cs` | Infrastructure | DATA: aranan permission |
| `PermissionHandler.cs` | Infrastructure | LOGIC: claim kontrolü |
| `PermissionPolicyProvider.cs` | Infrastructure | Dinamik policy üretimi |
| `AuthorizationServiceRegistration.cs` | Infrastructure | DI ve statik AddPolicy |
| `JwtTokenService.cs` | Infrastructure | BirthDate + permission claim'leri ekler |

**Neden requirement/handler Infrastructure'da?** `Microsoft.AspNetCore.Authorization` namespace'ine ihtiyaç var. Application katmanı ASP.NET'i bilmesin diye Infrastructure'a koyduk.

**Neden Policies/Permissions Application'da?** Bunlar saf sabit (string) — kimse bilmek zorunda. Hem API hem Infrastructure ulaşabilsin diye Application'da merkezi tutuyoruz.
