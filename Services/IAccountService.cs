using Dotnet6.Jwt.Dtos;

namespace Dotnet6.Jwt.Services;

public interface IAccountService
{
    Task<TokenDto> GetAuthToken(LoginDto login);

    Task<TokenDto> RenewTokens(RefreshTokenDto refreshTokenDto);
}