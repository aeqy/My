using Microsoft.EntityFrameworkCore;
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
}