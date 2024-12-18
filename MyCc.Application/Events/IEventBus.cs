using MyCc.Domain.Events;

namespace MyCc.Application.Events;

public interface IEventBus
{
    Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent;
    void Subscribe<TEvent, THandler>() where TEvent : IDomainEvent where THandler : IEventHandler<TEvent>;
}