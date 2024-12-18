using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MyCc.Infrastructure.Data;
using OpenIddict.Abstractions;

namespace MyCc.WebAPI.Extensions;

public static class OpenIddictExtensions
{
    public static IServiceCollection AddMyCcOpenIddictServer(this IServiceCollection service)
    {
        // 添加 Identity (如果需要)
        service.AddIdentity<IdentityUser<Guid>, IdentityRole<Guid>>()
            .AddEntityFrameworkStores<MyDbContext>()
            .AddDefaultTokenProviders();

        

        service.AddOpenIddict()

            // 注册 OpenIddict 核心组件。
            .AddCore(options =>
            {
                // 配置 OpenIddict 使用 Entity Framework Core 作为存储和模型。
                // 这使得 OpenIddict 可以将数据（如应用程序、令牌、授权）持久化到数据库中。
                options.UseEntityFrameworkCore()
                    .UseDbContext<MyDbContext>(); // 指定要使用的 DbContext 类型
                

                // 显式注册所有管理器


                // 启用 Quartz.NET 集成用于调度后台任务，例如清理过期令牌（如果需要）。
                // options.UseQuartz();
            })

            // 注册 OpenIddict 服务器组件。
            .AddServer(options =>
            {
                // 启用授权码流，这通常用于 Web 应用程序。
                options.AllowAuthorizationCodeFlow();

                // 启用客户端凭据流，用于机器对机器通信。
                options.AllowClientCredentialsFlow();

                // 启用刷新令牌流，允许客户端在不与用户交互的情况下请求新的访问令牌。
                options.AllowRefreshTokenFlow();

                // 设置由服务器签发的不同类型令牌的有效期。
                options.SetAccessTokenLifetime(TimeSpan.FromMinutes(30))
                    .SetRefreshTokenLifetime(TimeSpan.FromDays(7))
                    .SetIdentityTokenLifetime(TimeSpan.FromMinutes(5));

                // 配置 OpenIddict 服务器使用的端点 URI。
                options.SetTokenEndpointUris("/connect/token")
                    .SetAuthorizationEndpointUris("/connect/authorize")
                    .SetEndSessionEndpointUris("/connect/logout");

                // 使用 ASP.NET Core 内置的数据保护 API 来保护令牌。
                options.UseAspNetCore();

                // 禁用范围验证以支持动态范围（可选，仅在理解安全影响时使用）。
                // options.DisableScopeValidation();
                
                

                // 如果你需要支持刷新令牌，请启用它。
                options.AllowRefreshTokenFlow();

                // 注册应用程序类型。
                options.RegisterScopes("openid", "profile", "offline_access");

                // // 添加临时加密密钥（仅用于开发环境）
                // if (Environment.IsDevelopment())
                // {
                    options.AddEphemeralEncryptionKey();
                    options.AddEphemeralSigningKey();
                // }
                // else
                // {
                //     // 在生产环境中，你应该使用持久化的加密证书。
                //     var certificate = new X509Certificate2("path/to/certificate.pfx", "password");
                //     options.AddEncryptionCertificate(certificate);
                //     options.AddSigningCertificate(certificate);
                // }

                // 允许来自特定客户端的请求。
                options.AcceptAnonymousClients();
            })

            // 注册 OpenIddict 验证组件。
            .AddValidation(options =>
            {
                // 使用本地服务器作为验证提供程序。
                options.UseLocalServer();

                // 注意：这里不需要再次调用 UseEntityFrameworkCore()，
                // 因为已经在 AddCore 中配置过了。
            });
        

       
        


        // ... 将服务器配置移到这里
        return service;
    }

    
    
    /// <summary>
    /// 初始化应用程序数据库并创建默认OpenIddict客户端。
    /// </summary>
    /// <param name="app">IApplicationBuilder 实例。</param>
    /// <returns>一个表示操作完成的任务。</returns>
    public static async Task InitializeApplicationAsync(this IApplicationBuilder app)
    {
        using var scope = app.ApplicationServices.CreateScope();

        // 获取DbContext实例并应用迁移
        var dbContext = scope.ServiceProvider.GetRequiredService<MyDbContext>();
        await dbContext.Database.MigrateAsync();

        // 创建默认OpenIddict客户端
        await CreateDefaultOpenIddictClientAsync(scope);
    }

    private static async Task CreateDefaultOpenIddictClientAsync(IServiceScope scope)
    {
        var manager = scope.ServiceProvider.GetRequiredService<IOpenIddictApplicationManager>();

        if (await manager.FindByClientIdAsync("my-client") == null)
        {
            await manager.CreateAsync(new OpenIddictApplicationDescriptor
            {
                ClientId = "my-client",
                ClientSecret = "901564A5-E7FE-42CB-B10D-61EF6A8F379C",
                DisplayName = "My Client Application",
                RedirectUris = { new Uri("http://localhost:4200/callback") },
                PostLogoutRedirectUris = { new Uri("http://localhost:4200/") },
                Permissions =
                {
                    OpenIddictConstants.Permissions.Endpoints.Authorization,
                    OpenIddictConstants.Permissions.Endpoints.Token,
                    OpenIddictConstants.Permissions.GrantTypes.AuthorizationCode,
                    OpenIddictConstants.Permissions.GrantTypes.ClientCredentials,
                    OpenIddictConstants.Permissions.GrantTypes.RefreshToken,
                    OpenIddictConstants.Permissions.ResponseTypes.Code,
                    OpenIddictConstants.Permissions.Scopes.Email,
                    OpenIddictConstants.Permissions.Scopes.Profile,
                    OpenIddictConstants.Permissions.Scopes.Roles
                },
                Requirements =
                {
                    OpenIddictConstants.Requirements.Features.ProofKeyForCodeExchange
                }
            });
        }
    }

    }