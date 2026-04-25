using Microsoft.AspNetCore.Identity;

namespace InfraPlayground.Auth.Domain.Entities.Identity;

public class AppUser : IdentityUser<string>
{
    public string NameSurname { get; set; } = string.Empty;

    // BirthDate'i policy örneği için ekledik (MinimumAgeRequirement bunu kullanacak).
    // Nullable çünkü mevcut kullanıcı kayıtları olabilir; migration sırasında zorunlu kılmıyoruz.
    public DateOnly? BirthDate { get; set; }

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenEndDate { get; set; }
}
