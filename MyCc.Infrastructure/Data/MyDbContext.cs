using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using MyCc.Domain.Accounts;
using OpenIddict.EntityFrameworkCore.Models;

namespace MyCc.Infrastructure.Data;

public class MyDbContext(DbContextOptions<MyDbContext> options)
    : IdentityDbContext<IdentityUser<Guid>, IdentityRole<Guid>, Guid>(options)
{
    // 使用 OpenIddict 提供的内置实体
    public DbSet<OpenIddictEntityFrameworkCoreApplication> Applications { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreAuthorization> Authorizations { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreScope> Scopes { get; set; }
    public DbSet<OpenIddictEntityFrameworkCoreToken> Tokens { get; set; }
    
  
    protected override void OnModelCreating(ModelBuilder model)
    {
        base.OnModelCreating(model);
        
    }

    /// <summary>
    /// 控制台警告14000
    /// </summary>
    /// <param name="optionsBuilder"></param>
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.EnableSensitiveDataLogging(false);
    }
}