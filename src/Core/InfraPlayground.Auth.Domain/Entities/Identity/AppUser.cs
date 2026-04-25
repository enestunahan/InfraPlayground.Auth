using Microsoft.AspNetCore.Identity;

namespace InfraPlayground.Auth.Domain.Entities.Identity;

public class AppUser : IdentityUser<string>
{
    public string NameSurname { get; set; } = string.Empty;

    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenEndDate { get; set; }
}
