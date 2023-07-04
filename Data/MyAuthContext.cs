using Dotnet6.Jwt.Data.Entities;
using Microsoft.EntityFrameworkCore;

namespace Dotnet6.Jwt.Data;

public class MyAuthContext:DbContext
{
    public MyAuthContext(DbContextOptions<MyAuthContext> context):base(context)
    {
        
    }

    public DbSet<User> User { get; set; }
    public DbSet<RefreshToken> RefreshToken { get; set; }
}