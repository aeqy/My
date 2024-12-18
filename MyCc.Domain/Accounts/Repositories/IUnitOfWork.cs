namespace MyCc.Domain.Accounts.Repositories;

public interface IUnitOfWork
{
    Task<int> CommitAsync();
}