namespace MyCc.Domain.Events;

public class AccountCreatedEvent : IDomainEvent
{
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
    public Guid AccountId { get; }
    public string Email { get; }

    public AccountCreatedEvent(Guid accountId, string email)
    {
        AccountId = accountId;
        Email = email;
    }
}