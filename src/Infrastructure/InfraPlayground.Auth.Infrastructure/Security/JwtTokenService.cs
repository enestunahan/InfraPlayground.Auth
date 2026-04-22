using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using InfraPlayground.Auth.Application.Common.Security;
using InfraPlayground.Auth.Domain.Entities.Identity;
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
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.UserName ?? string.Empty),
            new(ClaimTypes.Email, user.Email ?? string.Empty),
            new("nameSurname", user.NameSurname)
        };

        var roles = await userManager.GetRolesAsync(user);
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var userClaims = await userManager.GetClaimsAsync(user);
        claims.AddRange(userClaims);

        return claims;
    }
}
