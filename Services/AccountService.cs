using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Dotnet6.Jwt.Data;
using Dotnet6.Jwt.Data.Entities;
using Dotnet6.Jwt.Dtos;
using Dotnet6.Jwt.Settings;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Dotnet6.Jwt.Services;

public class AccountService: IAccountService
{
    private readonly MyAuthContext _myAuthContext;
    private readonly TokenSettings _tokenSettings;
    public AccountService(MyAuthContext myAuthContext, IOptions<TokenSettings> tokenSettings)
    {
        _myAuthContext = myAuthContext;
        _tokenSettings = tokenSettings.Value;
    }

    public async Task<TokenDto> GetAuthToken(LoginDto login)
    {
        User user = await _myAuthContext.User
            .Where(_ => _.Email.ToLower() == login.Email.ToLower() &&
                        _.Password == login.Password).FirstOrDefaultAsync();

        if (user != null)
        {
            var accessToken = CreateJwtToken(user);
            var refreshToken = CreateRefreshToken();
            await InsertRefreshToken(user.Id, refreshToken);
            return new TokenDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken
            };
        }

        return null;
    }

    public async Task<TokenDto> RenewTokens(RefreshTokenDto refreshToken)
    {
        var userRefreshtoken = await _myAuthContext
            .RefreshToken.Where(_ => _.Token == refreshToken.Token && _.ExpirationDate >= DateTime.Now)
            .FirstOrDefaultAsync();

        if (userRefreshtoken==null)
        {
            return null;
        }

        var user = await _myAuthContext.User
            .Where(_ => _.Id == userRefreshtoken.UserId).FirstOrDefaultAsync();
        var newJwtToken = CreateJwtToken(user);
        var newRefreshToken = CreateRefreshToken();

        userRefreshtoken.Token = newRefreshToken;
        userRefreshtoken.ExpirationDate = DateTime.Now.AddDays(7);
        await _myAuthContext.SaveChangesAsync();

        return new TokenDto
        {
            AccessToken = newJwtToken,
            RefreshToken = newRefreshToken
        };
    }

    private string CreateJwtToken(User user)
    {
        var symmetricSecurityKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_tokenSettings.SecretKey));

        var cerdentials = new SigningCredentials(
            symmetricSecurityKey, SecurityAlgorithms.HmacSha256);

        var userClaims = new Claim[]
        {
            new Claim("email", user.Email),
            new Claim("phone", user.PhoneNumber)
        };

        var jwtToken = new JwtSecurityToken(
            issuer: _tokenSettings.Issure,
            expires: DateTime.Now.AddMinutes(600),
            signingCredentials: cerdentials,
            claims: userClaims,
            audience: _tokenSettings.Audience
        );
        string token = new JwtSecurityTokenHandler().WriteToken(jwtToken);
        return token;
    }

    private string CreateRefreshToken()
    {
        var tokenBytes = RandomNumberGenerator.GetBytes(64);
        var refreshToken = Convert.ToBase64String(tokenBytes);

        var tokenIsInUse = _myAuthContext
            .RefreshToken.Any(_ => _.Token == refreshToken);

        if (tokenIsInUse)
        {
            return CreateRefreshToken();
        }

        return refreshToken;
    }

    
    private async Task InsertRefreshToken(int userId, string refreshToken)
    {
        var newRefreshToken = new RefreshToken
        {
            UserId = userId,
            Token = refreshToken,
            ExpirationDate = DateTime.Now.AddDays(14)
        };

        _myAuthContext.RefreshToken.Add(newRefreshToken);
        await _myAuthContext.SaveChangesAsync();
    }
    
}