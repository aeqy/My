using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using MyCc.Infrastructure.Data;

namespace MyCc.WebAPI.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection ConfigureServicesDatabase(this IServiceCollection services,
        IConfiguration configuration)
    {
        // 配置数据库上下文，使用 PostgreSQL 数据库
        services.AddDbContext<MyDbContext>(options =>
        {
            // 从配置文件中获取数据库连接字符串，使用 Npgsql 来连接 PostgreSQL 数据库
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));
        });
        
        return services;    // 返回服务集合
    }
    
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true, // 是否验证颁发者
                    ValidateAudience = true, // 是否验证受众
                    ValidateLifetime = true, // 是否验证过期时间
                    ValidateIssuerSigningKey = true, // 是否验证签名密钥
                    ValidIssuer = configuration.GetConnectionString("Jwt:Issuer"), // 颁发者
                    ValidAudience = configuration.GetConnectionString("Jwt:Audience"), // 受众
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(configuration.GetConnectionString("Jwt:Key") ?? string.Empty)) // 签名密钥
                };
            });

        return services;
    }
}