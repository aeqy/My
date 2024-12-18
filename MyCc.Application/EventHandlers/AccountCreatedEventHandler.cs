using Microsoft.Extensions.Logging;
using MyCc.Application.Events;
using MyCc.Domain.Events;

namespace MyCc.Application.EventHandlers;

public class AccountCreatedEventHandler(
    ILogger<AccountCreatedEventHandler> logger)
    : IEventHandler<AccountCreatedEvent>
{
    public async Task HandleAsync(AccountCreatedEvent @event)
    {
        // 在这里处理账户创建事件，例如发送欢迎邮件
        logger.LogInformation($"账户 {@event.Email} 已创建，ID：{@event.AccountId}");
        await Task.CompletedTask;
    }
}