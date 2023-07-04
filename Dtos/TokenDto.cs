using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Dotnet6.Jwt.Dtos;

public class TokenDto
{
    public string AccessToken { get; set; }
    public string RefreshToken { get; set; }
}