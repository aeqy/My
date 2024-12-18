namespace MyCc.Domain.Events;

public interface IDomainEvent
{
    DateTime OccurredOn { get; }
}