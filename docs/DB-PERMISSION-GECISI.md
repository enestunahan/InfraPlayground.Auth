# DB Tabanli Permission Gecisi

Bu dokuman, projede role->permission bilgisini koddan veritabanina tasima degisikliklerini ozetler.

## Ne Degisti?

1. Yeni domain modelleri eklendi:
   - `Permission` (Id, Code, Description)
   - `RolePermission` (RoleId, PermissionId)

2. `AppRole` icine navigation eklendi:
   - `ICollection<RolePermission> RolePermissions`

3. `DbContext` guncellendi:
   - `DbSet<Permission> Permissions`
   - `DbSet<RolePermission> RolePermissions`

4. EF konfigurasyonlari eklendi:
   - `PermissionConfiguration`
   - `RolePermissionConfiguration`

5. Permission claim uretimi degisti:
   - Eskiden: JWT icin permission listesi `RolePermissions.Map` uzerinden koddan cikariliyordu.
   - Simdi: `IPermissionLookupService` ile veritabanindan okunuyor.
   - Uygulama davranisi degismedi: endpointler yine JWT icindeki `permission` claim'ini kontrol ediyor.

6. Seeder guncellendi:
   - `Permissions` tablosuna temel permission kayitlari idempotent bicimde ekleniyor.
   - `RolePermissions` tablosuna varsayilan rol-permission eslesmeleri sadece ilk bootstrap'ta (tablo bossa) ekleniyor.
   - Mevcut kullanici/rol seeding akisi korunuyor.

## Eklenen Dosyalar

- `src/Core/InfraPlayground.Auth.Domain/Entities/Identity/Permission.cs`
- `src/Core/InfraPlayground.Auth.Domain/Entities/Identity/RolePermission.cs`
- `src/Core/InfraPlayground.Auth.Application/Common/Authorization/IPermissionLookupService.cs`
- `src/Infrastructure/InfraPlayground.Auth.Persistence/Configurations/PermissionConfiguration.cs`
- `src/Infrastructure/InfraPlayground.Auth.Persistence/Configurations/RolePermissionConfiguration.cs`
- `src/Infrastructure/InfraPlayground.Auth.Persistence/Security/PermissionLookupService.cs`

## Guncellenen Dosyalar

- `src/Core/InfraPlayground.Auth.Domain/Entities/Identity/AppRole.cs`
- `src/Infrastructure/InfraPlayground.Auth.Persistence/Contexts/InfraPlaygroundAuthDbContext.cs`
- `src/Infrastructure/InfraPlayground.Auth.Persistence/PersistenceServiceRegistration.cs`
- `src/Infrastructure/InfraPlayground.Auth.Infrastructure/Security/JwtTokenService.cs`
- `src/Infrastructure/InfraPlayground.Auth.Persistence/Seed/IdentitySeeder.cs`

## Migration Notu

Bu degisiklikler icin migration olusturulmasi gerekir:

```bash
dotnet ef migrations add add_db_permissions \
  --project src/Infrastructure/InfraPlayground.Auth.Persistence/InfraPlayground.Auth.Persistence.csproj \
  --startup-project src/Presentation/InfraPlayground.Auth.API/InfraPlayground.Auth.API.csproj \
  --context InfraPlaygroundAuthDbContext \
  --output-dir Migrations
```

Ardindan:

```bash
dotnet ef database update \
  --project src/Infrastructure/InfraPlayground.Auth.Persistence/InfraPlayground.Auth.Persistence.csproj \
  --startup-project src/Presentation/InfraPlayground.Auth.API/InfraPlayground.Auth.API.csproj
```

## Davranis Ozeti

- Runtime authorization modeli degismedi:
  - API request geldiginde DB'ye permission icin gitmiyoruz.
  - JWT icindeki `permission` claim'leri kontrol ediliyor.

- Degisen nokta:
  - JWT claim seti artik login/refresh aninda DB'den uretiliyor.
  - Bu sayede role->permission kaynagi kod yerine veritabani oluyor.
