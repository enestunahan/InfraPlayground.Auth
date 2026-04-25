# Auth Kılavuzu — Yeni Başlayan İçin

Bu doküman, projedeki kimlik doğrulama (authentication) ve yetkilendirme (authorization) akışını sıfırdan anlamak için hazırlandı. Hiç JWT görmemiş birinin bile takip edebileceği şekilde yazıldı.

---

## 1. Authentication vs Authorization

İkisi sürekli karıştırılır, aslında farklı şeyler:

- **Authentication (Kimlik Doğrulama):** "Sen kimsin?" — Kullanıcı adı + şifre verirsin, sistem "evet bu sensin" der.
- **Authorization (Yetkilendirme):** "Bunu yapmaya hakkın var mı?" — Kimliğin doğrulandıktan sonra, belirli bir endpoint'e erişim izninin olup olmadığına bakılır.

Sıra her zaman şu şekilde: önce authentication, sonra authorization.

---

## 2. JWT (JSON Web Token) — Neden ve Nasıl?

### Sorun

Geleneksel (cookie + session) yöntemde:
- Kullanıcı login olur, sunucu bir session oluşturur (RAM'de veya DB'de).
- Her request'te sunucu "bu session hâlâ geçerli mi?" diye kontrol etmek zorunda.
- Çoklu sunucuya ölçeklediğinde session'ı nerede tutacağın problem olur (sticky session, Redis, vs).

### Çözüm: Stateless Token

JWT, "sunucu hiçbir şey hatırlamasın, kullanıcı kendi kimliğini her request'te yanında getirsin" fikrine dayanır.

**Bir JWT 3 parçadan oluşur, noktayla ayrılır:**

```
eyJhbGciOiJIUzI1NiJ9.eyJzdWIiOiIxMjMiLCJyb2xlIjoiQWRtaW4ifQ.SflKxwRJSMeKKF2QT4fwpMeJf36POk6yJV_adQssw5c
     └─── Header ───┘ └─────────── Payload ────────────┘ └─────────────── Signature ──────────────┘
```

1. **Header:** Hangi algoritmayla imzalandı (`HS256` vb). Base64-encoded JSON.
2. **Payload (Claims):** Kullanıcı bilgisi, roller, son kullanma tarihi. Base64-encoded JSON. **Şifrelenmemiş, herkes okuyabilir** — buraya şifre gibi hassas bilgi KOYMAZSIN.
3. **Signature:** İlk iki parçanın `SecurityKey` ile hash'lenmiş hâli. Bu imza sayesinde token'ı kimse değiştiremez; değiştirse imza tutmaz.

### Bu Projede Token Nasıl Üretiliyor?

Bak [JwtTokenService.cs](../src/Infrastructure/InfraPlayground.Auth.Infrastructure/Security/JwtTokenService.cs):

```csharp
var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

var claims = await BuildClaimsAsync(user); // userId, roller, email vb

var jwt = new JwtSecurityToken(
    issuer: _tokenOptions.Issuer,         // "Bu token'ı ben ürettim"
    audience: _tokenOptions.Audience,     // "Bu token şu istemci için"
    expires: accessTokenExpiration,       // Geçerlilik sonu
    notBefore: DateTime.UtcNow,           // Şu tarihten önce geçersiz
    claims: claims,
    signingCredentials: signingCredentials);

var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
```

**Claims (iddialar)** = token'ın içindeki kullanıcı bilgileri. Örneğin `ClaimTypes.Role = "Admin"` yazarsan, doğrulama yapan taraf "bu kullanıcı Admin" diye kabul eder.

### SecurityKey Neden Önemli?

Token'ı imzalayan gizli anahtar. `appsettings.json` içinde:

```json
"SecurityKey": "L6LiC5FafhgH0XrXgi/VQVTSHS1vIJ4n+y3OHb7Q2q7Om3JwqS7atA5eP2LnQc9LoQeXS4oiCBivhJDsxzlsag=="
```

Bu anahtar sızarsa, saldırgan istediği token'ı üretip "ben Admin'im" diyebilir. O yüzden **prod'da asla git'e commit'lenmez**, environment variable veya secret manager'da tutulur. Bu projede öğrenme amaçlı açık duruyor.

---

## 3. Access Token + Refresh Token — Neden İkisi?

### Access Token (15 dakika)
- Kısa ömürlü, her request'te header'da gönderilir.
- Çalındığında zarar sınırlıdır; birkaç dakika sonra geçersiz olur.

### Refresh Token (7 gün)
- Uzun ömürlü, DB'de saklanır (`AppUser.RefreshToken`).
- Access token dolduğunda, refresh token ile yeni bir access token alınır — kullanıcı tekrar şifre girmek zorunda kalmaz.
- Çalındığında riski daha büyük olduğu için DB'deki kayıtla eşleşip eşleşmediği kontrol edilir.

### Akış

```
1. Kullanıcı login  → Access(15dk) + Refresh(7gün) alır
2. Her API çağrısı  → Access token gönder
3. Access süresi doldu → Refresh token ile /auth/refresh-token-login çağır
4. Yeni access + yeni refresh al (rotation: eski refresh artık geçersiz)
5. Refresh de doldu / logout → Yeniden login gerek
```

Projede refresh token her yenilenmede değişir; `RefreshTokenLoginCommandHandler` yeni bir tane üretip `AppUser.RefreshToken`'ı günceller. Bu "rotation" güvenlik için önemli — eski refresh token çalındıysa bir kez kullanılır, sonra işe yaramaz.

---

## 4. ASP.NET Core Identity — Nedir, Niye Var?

Tekerleği yeniden icat etmemek için var. Identity sana hazır:
- `AspNetUsers` / `AspNetRoles` tabloları
- Şifre hash'leme (PBKDF2)
- Hesap kilitleme (brute force koruması)
- Email confirm / password reset altyapısı
- `UserManager<T>` / `RoleManager<T>` servisleri

Projedeki kurulum ([PersistenceServiceRegistration.cs](../src/Infrastructure/InfraPlayground.Auth.Persistence/PersistenceServiceRegistration.cs)):

```csharp
services.AddIdentityCore<AppUser>(options =>
{
    options.User.RequireUniqueEmail = true;
    options.Password.RequiredLength = 6;
    // ...
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
})
.AddRoles<AppRole>()
.AddEntityFrameworkStores<InfraPlaygroundAuthDbContext>();
```

- `AddIdentityCore` vs `AddIdentity`: **Core**, minimalist versiyondur — sadece user/role yönetimi. `AddIdentity` bunun üzerine cookie auth vs ekler. Biz JWT kullandığımız için Core yeterli.
- `AddEntityFrameworkStores`: Identity'nin tablolara EF üzerinden erişmesini sağlar.

### AppUser'ı Neden Extend Ettik?

```csharp
public class AppUser : IdentityUser<string>
{
    public string NameSurname { get; set; } = string.Empty;
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenEndDate { get; set; }
}
```

Identity'nin varsayılan `IdentityUser`'ında `NameSurname` ve `RefreshToken` yok. Kendi alanlarımızı eklemek için inherit ettik.

---

## 5. Middleware Pipeline — Sıralama Kritik

[Program.cs](../src/Presentation/InfraPlayground.Auth.API/Program.cs) içinde sırayla:

```csharp
app.UseMiddleware<ExceptionHandlingMiddleware>();  // 1
app.UseHttpsRedirection();                          // 2
app.UseAuthentication();                            // 3
app.UseAuthorization();                             // 4
app.MapControllers();                               // 5
```

Her request yukarıdan aşağıya bu borudan geçer. **Sıralama önemli** — alttakiler üsttekinin yaptığına güvenir.

### `UseAuthentication()` — Ne Yapar?

- Request'te `Authorization: Bearer <token>` header'ı var mı bakar.
- Varsa token'ı **doğrular**:
  - İmza geçerli mi? (`SecurityKey` ile tekrar hash'leyip karşılaştırır)
  - Süresi dolmuş mu? (`exp` claim)
  - Issuer/Audience eşleşiyor mu?
- Geçerliyse, token içindeki claim'leri `HttpContext.User` (ClaimsPrincipal) içine yükler.
- **Geçersizse hiçbir şey yapmaz** — kullanıcı anonymous kalır, hata fırlatmaz.

Kurulumu [AuthenticationExtensions.cs](../src/Presentation/InfraPlayground.Auth.API/Extensions/AuthenticationExtensions.cs):

```csharp
options.TokenValidationParameters = new TokenValidationParameters
{
    ValidateIssuer = true,
    ValidateAudience = true,
    ValidateLifetime = true,
    ValidateIssuerSigningKey = true,
    ValidIssuer = tokenOptions.Issuer,
    ValidAudience = tokenOptions.Audience,
    IssuerSigningKey = new SymmetricSecurityKey(...),
    ClockSkew = TimeSpan.Zero, // Süresi dolmuş token'a 5dk tolerans VERME
    NameClaimType = ClaimTypes.Name,
    RoleClaimType = ClaimTypes.Role
};
```

`ClockSkew = TimeSpan.Zero`: Varsayılan 5 dakikadır (sunucu saatleri uyumsuz olabilir diye). Geliştirirken `0` yap ki token gerçekten süresi dolduğunda reddedilsin.

### `UseAuthorization()` — Ne Yapar?

Controller/endpoint'teki `[Authorize]` attribute'larına bakar. User authenticated mi, rolü yeterli mi — karar verir.

- Authenticated değilse → **401 Unauthorized**
- Authenticated ama rol yetmiyorsa → **403 Forbidden**

### Sıralama Niye Bu Kadar Önemli?

`UseAuthorization`'dan ÖNCE `UseAuthentication` çağırmazsan, authorization `HttpContext.User` boş olduğu için **herkesi anonymous sanar** ve her şeyi reddeder. `app.MapControllers()`'ı ikisinden önce yazarsan hiç çalışmaz.

### ExceptionHandlingMiddleware

[ExceptionHandlingMiddleware.cs](../src/Presentation/InfraPlayground.Auth.API/Middlewares/ExceptionHandlingMiddleware.cs) en başta duruyor çünkü aşağıdaki tüm middleware'lerden fırlayan hataları yakalayıp JSON response'a çevirir:

- `AuthenticationFailedException` → 401
- `NotFoundException` → 404
- `BusinessException` → 400
- Diğer her şey → 500 (kullanıcıya generic mesaj, log'a gerçek hata)

---

## 6. `[Authorize]` Attribute'u — Senaryolar

[HomeController.cs](../src/Presentation/InfraPlayground.Auth.API/Controllers/HomeController.cs) bilerek birden fazla senaryo içerir:

| Endpoint | Attribute | Kim Erişebilir? |
|---|---|---|
| `/api/home/public` | `[AllowAnonymous]` | Herkes (token gerekmez) |
| `/api/home/me` | `[Authorize]` | Token'ı geçerli her kullanıcı |
| `/api/home/admin-only` | `[Authorize(Roles = "Admin")]` | Sadece Admin |
| `/api/home/admin-or-editor` | `[Authorize(Roles = "Admin,Editor")]` | Admin VEYA Editor |
| `/api/home/user-and-above` | `[Authorize(Roles = "Admin,Editor,User")]` | Bu üç rolden biri |
| `/api/admin/books/*` | Controller-level `[Authorize(Roles = "Admin")]` | Sadece Admin |

**Önemli:** `Roles = "Admin,Editor"` virgülle yazılırsa **OR** anlamına gelir (ikisinden biri yeter). AND için attribute'u iki kez yazman gerek:

```csharp
[Authorize(Roles = "Admin")]
[Authorize(Roles = "Editor")]  // İkisine birden sahip olmalı
```

Bu hantal — gerçek AND senaryoları için **policy** kullanılır (bir sonraki aşama).

---

## 7. Login Handler — Adım Adım

[LoginCommandHandler.cs](../src/Core/InfraPlayground.Auth.Application/Features/Auth/Commands/Login/LoginCommandHandler.cs):

```csharp
// 1. Kullanıcıyı bul (username VEYA email ile)
var user = await userManager.FindByNameAsync(request.UserNameOrEmail)
           ?? await userManager.FindByEmailAsync(request.UserNameOrEmail);

if (user is null)
    throw new AuthenticationFailedException("Kullanıcı adı veya şifre hatalı.");
// Not: "Kullanıcı bulunamadı" yerine "kullanıcı adı veya şifre hatalı" diyoruz —
//      saldırgan hangi kullanıcı adlarının var olduğunu öğrenmesin diye (user enumeration)

// 2. Hesap kilitli mi? (çok fazla yanlış denemeden sonra Identity otomatik kilitler)
if (await userManager.IsLockedOutAsync(user))
    throw new AuthenticationFailedException("Hesap geçici olarak kilitlendi...");

// 3. Şifreyi doğrula (Identity hash'i kendi yapar, sen düz şifre verirsin)
var isPasswordValid = await userManager.CheckPasswordAsync(user, request.Password);
if (!isPasswordValid)
{
    await userManager.AccessFailedAsync(user); // Sayacı artır — lockout'a doğru
    throw new AuthenticationFailedException("Kullanıcı adı veya şifre hatalı.");
}

// 4. Başarılı giriş — fail sayacını sıfırla
await userManager.ResetAccessFailedCountAsync(user);

// 5. Token üret
var token = await tokenService.CreateTokenAsync(user);

// 6. Refresh token'ı DB'ye kaydet (yenileme sırasında eşleştirme için)
user.RefreshToken = token.RefreshToken;
user.RefreshTokenEndDate = token.RefreshTokenExpiration;
await userManager.UpdateAsync(user);

return token;
```

---

## 8. `ICurrentUserService` — Nedir?

[CurrentUserService.cs](../src/Infrastructure/InfraPlayground.Auth.Infrastructure/Security/CurrentUserService.cs) basit bir wrapper:

```csharp
private ClaimsPrincipal? Principal => httpContextAccessor.HttpContext?.User;
public string? UserId => Principal?.FindFirstValue(ClaimTypes.NameIdentifier);
```

Handler'larda "şu an login olan kullanıcı kim?" sorusunu temiz bir şekilde sormak için. Application katmanı `HttpContext`'i doğrudan bilmesin diye araya bir abstraction konuldu — bu sayede Application katmanı ASP.NET bağımlılığından bağımsız kalır.

---

## 9. Seed Data — Test Kullanıcıları

[IdentitySeeder.cs](../src/Infrastructure/InfraPlayground.Auth.Persistence/Seed/IdentitySeeder.cs) uygulama başlarken 4 rol ve 4 kullanıcı oluşturur:

| UserName | Rol | Şifre |
|---|---|---|
| `admin` | Admin | `Password123!` |
| `enes.editor` | Editor | `Password123!` |
| `enes.user` | User | `Password123!` |
| `enes.viewer` | Viewer | `Password123!` |

Postman/curl ile hızlı test için kullanırsın.

---

## 10. Hızlı Test Akışı

```bash
# 1. Login
curl -X POST http://localhost:5106/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"userNameOrEmail":"admin","password":"Password123!"}'
# → { "accessToken": "eyJ...", "refreshToken": "...", ... }

# 2. Korumalı endpoint
curl http://localhost:5106/api/home/me \
  -H "Authorization: Bearer eyJ..."

# 3. Refresh
curl -X POST http://localhost:5106/api/auth/refresh-token-login \
  -H "Content-Type: application/json" \
  -d '{"refreshToken":"..."}'
```

---

## 11. Sonraki Aşama: Policy

Şu an rollerle yetkilendiriyoruz: `[Authorize(Roles = "Admin")]`. Ama gerçek hayatta bu yetersiz kalıyor:

- "Kullanıcı Admin VE 18 yaşından büyük olmalı"
- "Kullanıcı sadece kendi oluşturduğu kaynağı silebilir"
- "Belirli bir şirkete ait kullanıcılar erişebilir"

Bu tür kuralları **Policy** ile yazıyoruz. Sonraki aşamada ona geçeceğiz.
