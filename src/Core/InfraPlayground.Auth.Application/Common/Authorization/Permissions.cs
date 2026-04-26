namespace InfraPlayground.Auth.Application.Common.Authorization;

/// <summary>
/// İzin (permission) sabitleri.
///
/// Permission, role'dan daha ince kırılımlı yetki birimidir:
///   - Role:       "Admin"       (kullanıcının kim olduğunu söyler)
///   - Permission: "Books.Create" (kullanıcının ne yapabileceğini söyler)
///
/// Endpoint'lerde [Authorize(Policy = Permissions.Books.Create)] gibi kullanıyoruz.
/// Bu policy önceden register edilmedi — PermissionPolicyProvider runtime'da
/// "Permissions." prefix'iyle başlayan her policy'yi otomatik üretiyor.
///
/// Kullanıcının sahip olduğu permission'lar JWT içine "permission" claim'i
/// olarak basılır (bkz. JwtTokenService). Yetki kontrolünde DB'ye gidilmez.
/// </summary>
public static class Permissions
{
    public const string ClaimType = "permission";

    public static class Books
    {
        public const string Create = "Permissions.Books.Create";
        public const string Read   = "Permissions.Books.Read";
        public const string Update = "Permissions.Books.Update";
        public const string Delete = "Permissions.Books.Delete";
    }

    public static class PermissionManagement
    {
        public const string Create = "Permissions.PermissionManagement.Create";
        public const string Read   = "Permissions.PermissionManagement.Read";
        public const string Update = "Permissions.PermissionManagement.Update";
        public const string Delete = "Permissions.PermissionManagement.Delete";
    }
}
