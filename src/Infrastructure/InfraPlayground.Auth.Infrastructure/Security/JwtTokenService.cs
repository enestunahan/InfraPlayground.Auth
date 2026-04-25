using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Globalization;
using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Common.Security;
using InfraPlayground.Auth.Domain.Entities.Identity;
using InfraPlayground.Auth.Infrastructure.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace InfraPlayground.Auth.Infrastructure.Security;

public sealed class JwtTokenService(
    IOptions<JwtTokenOptions> tokenOptions,
    UserManager<AppUser> userManager) : ITokenService
{
    private readonly JwtTokenOptions _tokenOptions = tokenOptions.Value;

    public async Task<TokenDto> CreateTokenAsync(AppUser user)
    {
        var accessTokenExpiration = DateTime.UtcNow.AddMinutes(_tokenOptions.AccessTokenExpirationMinutes);
        var refreshTokenExpiration = DateTime.UtcNow.AddDays(_tokenOptions.RefreshTokenExpirationDays);

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_tokenOptions.SecurityKey));
        var signingCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = await BuildClaimsAsync(user);

        var jwt = new JwtSecurityToken(
            issuer: _tokenOptions.Issuer,
            audience: _tokenOptions.Audience,
            expires: accessTokenExpiration,
            notBefore: DateTime.UtcNow,
            claims: claims,
            signingCredentials: signingCredentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(jwt);
        var refreshToken = CreateRefreshToken();

        return new TokenDto(accessToken, accessTokenExpiration, refreshToken, refreshTokenExpiration);
    }

    public string CreateRefreshToken()
    {
        var bytes = RandomNumberGenerator.GetBytes(64);
        return Convert.ToBase64String(bytes);
    }

    private async Task<List<Claim>> BuildClaimsAsync(AppUser user)
    {
        // Standart claim'ler — kim olduğu, hangi token olduğu, vs.
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("nameSurname", user.NameSurname)
        };

        // BirthDate claim'i — MinimumAgeHandler bunu okuyacak.
        // ISO 8601 (yyyy-MM-dd) formatında basıyoruz; lokalizasyon problemi olmasın.
        if (user.BirthDate.HasValue)
        {
            claims.Add(new Claim(
                MinimumAgeHandler.BirthDateClaimType,
                user.BirthDate.Value.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)));
        }

        // Roller
        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        // Permission'lar — kullanıcının rollerinden türetilir, claim olarak basılır.
        // Authorization sırasında PermissionHandler bu claim'leri kontrol eder,
        // DB'ye gitmez. Stateless authorization'ın anahtarı bu.
        foreach (var permission in RolePermissions.GetPermissionsForRoles(roles))
        {
            claims.Add(new Claim(Permissions.ClaimType, permission));
        }

        // Identity'nin kendi sakladığı ek claim'ler (GetClaimsAsync) — örn:
        // adminin "department=IT" gibi özel claim'i varsa burası getirir.
        var userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;
    }
}
