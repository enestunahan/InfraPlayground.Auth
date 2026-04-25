namespace InfraPlayground.Auth.Application.Common.Authorization;

/// <summary>
/// Hangi role'ün hangi permission'lara sahip olduğunu tutan harita.
///
/// Bu, "kod tabanlı" permission yönetimidir. Bir sonraki seviyede bunu DB'ye
/// taşıyıp admin paneliyle yönetebileceksin. Şu an sabit kod, çünkü amaç
/// önce mantığı oturtmak.
///
/// Login olduğunda JwtTokenService bu haritaya bakıp kullanıcının rollerinden
/// türetilen permission'ları JWT claim'lerine ekler. Her rol birden fazla
/// permission'a sahip olabilir; permission'lar farklı rollerde tekrar edebilir.
/// </summary>
public static class RolePermissions
{
    public static readonly IReadOnlyDictionary<string, string[]> Map =
        new Dictionary<string, string[]>
        {
            // Admin her şeyi yapabilir
            [AppRoles.Admin] = new[]
            {
                Permissions.Books.Create,
                Permissions.Books.Read,
                Permissions.Books.Update,
                Permissions.Books.Delete
            },

            // Editor silemez ama oluşturup güncelleyebilir
            [AppRoles.Editor] = new[]
            {
                Permissions.Books.Create,
                Permissions.Books.Read,
                Permissions.Books.Update
            },

            // User sadece okuyabilir
            [AppRoles.User] = new[]
            {
                Permissions.Books.Read
            },

            // Viewer da sadece okuyabilir (ama farklı UI/akış için ayrı rol)
            [AppRoles.Viewer] = new[]
            {
                Permissions.Books.Read
            }
        };

    /// <summary>
    /// Birden fazla rolün permission'larını birleştirip tekilleştirir.
    /// Aynı permission iki rolde varsa bir kez döner.
    /// </summary>
    public static IEnumerable<string> GetPermissionsForRoles(IEnumerable<string> roles)
    {
        var bag = new HashSet<string>(StringComparer.Ordinal);
        foreach (var role in roles)
        {
            if (Map.TryGetValue(role, out var perms))
                foreach (var p in perms)
                    bag.Add(p);
        }
        return bag;
    }
}
