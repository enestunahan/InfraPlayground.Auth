namespace InfraPlayground.Auth.Application.Common.Security;

public sealed record TokenDto(
    string AccessToken,
    DateTime AccessTokenExpiration,
    string RefreshToken,
    DateTime RefreshTokenExpiration);
