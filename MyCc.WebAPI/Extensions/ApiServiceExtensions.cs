using MyCc.Application.EventHandlers;
using MyCc.Application.Events;
using MyCc.Application.Identity;
using MyCc.Application.Services;
using MyCc.Domain.Accounts.Repositories;
using MyCc.Domain.Events;
using MyCc.Infrastructure.Repositories;

namespace MyCc.WebAPI.Extensions;

public static class ApiServiceExtensions
{
    public static IServiceCollection AddApiServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<IAccountRepository, EfAccountRepository>(); // 注入账户存储库
        services.AddScoped<IUnitOfWork, UnitOfWork>(); // 注入单元工作
        // services.AddScoped<AccountService>(); // 注入账户应用服务 //TODO 添加就无法启动
        // services.AddScoped<UserAppService>(); // 注入用户应用服务 //TODO 添加就无法启动
        services.AddScoped<IEventBus, InMemoryEventBus>(); // 注入事件总线
        services.AddTransient<IEventHandler<AccountCreatedEvent>, AccountCreatedEventHandler>(); // 注入账户创建事件处理器
        return services; // 返回服务集合以支持链式调用
    }
}