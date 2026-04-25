using InfraPlayground.Auth.Domain.Entities.Identity;

namespace InfraPlayground.Auth.Application.Common.Security;

public interface ITokenService
{
    Task<TokenDto> CreateTokenAsync(AppUser user);
    string CreateRefreshToken();
}
