using MyCc.Domain.Accounts.Repositories;
using MyCc.Infrastructure.Data;

namespace MyCc.Infrastructure.Repositories;

public class UnitOfWork(MyDbContext dbContext) : IUnitOfWork
{
    public async Task<int> CommitAsync()
    {
        return await dbContext.SaveChangesAsync();
    }
}