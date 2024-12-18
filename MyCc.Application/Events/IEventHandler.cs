using MyCc.Domain.Events;

namespace MyCc.Application.Events;

public interface IEventHandler<TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent @event);
}