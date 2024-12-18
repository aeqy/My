using MyCc.Domain.Events;

namespace MyCc.Application.Events;

public class InMemoryEventBus : IEventBus
{
    private readonly Dictionary<Type, List<Func<IDomainEvent, Task>>> _handlers = new();

    public async Task PublishAsync<TEvent>(TEvent @event) where TEvent : IDomainEvent
    {
        if (_handlers.TryGetValue(typeof(TEvent), out var handlers))
        {
            foreach (var handler in handlers)
            {
                await handler(@event);
            }
        }
    }

    public void Subscribe<TEvent, THandler>()
        where TEvent : IDomainEvent
        where THandler : IEventHandler<TEvent>
    {
        if (!_handlers.ContainsKey(typeof(TEvent)))
        {
            _handlers[typeof(TEvent)] = new List<Func<IDomainEvent, Task>>();
        }

        _handlers[typeof(TEvent)].Add(async (domainEvent) =>
        {
            var handler = Activator.CreateInstance<THandler>();
            await handler.HandleAsync((TEvent)domainEvent);
        });
    }
}